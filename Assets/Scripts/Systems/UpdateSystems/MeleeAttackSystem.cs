using Components;
using ECS;
using Tags;
using UnityEngine;

[System(ESystemCategory.Update)]
public class MeleeAttackSystem : EcsSystem
{
    private int _startAttackFilterId;
    private int _tagFallThroughFilterId;
    private int _filterId;

    private const int OverlapsCount = 16;
    private Collider[] _overlapResults;

    public MeleeAttackSystem(EcsWorld world)
    {
        _startAttackFilterId = world.RegisterFilter(new BitMask(Id<AttackEvent>(),
                                                                Id<MeleeWeapon>(),
                                                                Id<Owner>()));

        _tagFallThroughFilterId = world.RegisterFilter(new BitMask(Id<CharacterTag>(), Id<CurrentWeapon>(), Id<MeleeAttackEvent>()));

        _filterId = world.RegisterFilter(new BitMask(Id<MeleeAttackEvent>(),
                                                     Id<MeleeWeapon>(),
                                                     Id<DamageComponent>(),
                                                     Id<ReachComponent>(),
                                                     Id<AttackAngle>()));

        _overlapResults = new Collider[OverlapsCount];
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_startAttackFilterId))
        {
            var ownerId = world.GetComponent<Owner>(id).entity.GetId();
            if (world.Have<Animator>(ownerId))
            {
                var animator = world.GetComponent<Animator>(ownerId);
                animator.SetTrigger(world.Have<DefaultMeleeWeapon>(id) ? "IsPunching" : "IsMelee");
            }
        }

        foreach (var id in world.Enumerate(_tagFallThroughFilterId))
        {
            var weaponId = world.GetComponent<CurrentWeapon>(id).entity.GetId();
            world.Add(weaponId, world.GetComponent<MeleeAttackEvent>(id));
            world.Remove<MeleeAttackEvent>(id);
        }

        foreach (var id in world.Enumerate(_filterId))
        {
            Debug.Log("melee attack!");
            var attack = world.GetComponent<MeleeAttackEvent>(id);
            world.Remove<MeleeAttackEvent>(id);
            var attackDistance = world.GetComponent<ReachComponent>(id).distance;
            var position = attack.position;
            //TODO: set masks for physics
            var overlapCount = Physics.OverlapSphereNonAlloc(position, attackDistance, _overlapResults);
            if (overlapCount <= 0)
                continue;

            var angle = world.GetComponent<AttackAngle>(id).angle;
            overlapCount = MoveAllSuitableCollidersToFront(world, _overlapResults, overlapCount, attack, angle);
            if (overlapCount <= 0)
                continue;

            //TODO: sort by cw/ccw for side attacks and by min angle for front attack
            SortCollidersByDistance(_overlapResults, position, overlapCount);

            for (int i = 0; i < overlapCount; i++)
            {
                var targetPos = _overlapResults[i].transform.position;
                if (Physics.Raycast(position, targetPos - position, out RaycastHit hit, attackDistance) &&
                    hit.collider == _overlapResults[i])
                {
                    Debug.Log("melee hit! " + hit.collider.gameObject.name);
                    var targetId = hit.collider.gameObject.GetComponent<EntityView>().Id;
                    world.GetComponentByRef<HealthComponent>(targetId).health -= world.GetComponent<DamageComponent>(id).damage;

                    //TODO: remove break if area attack needed
                    break;
                }
            }
        }
    }

    private static bool IsValidView(EcsWorld world, Collider collider)
    {
        var targetView = collider.gameObject.GetComponent<EntityView>();
        if (targetView == null)
            return false;

        var targetEntity = targetView.Entity;
        if (!world.IsEntityValid(targetEntity))
            return false;

        var tagetId = targetEntity.GetId();
        if (!world.Have<HealthComponent>(tagetId) || world.Have<Pickup>(tagetId))
            return false;

        return true;
    }

    private static int MoveAllSuitableCollidersToFront(EcsWorld world,
                                                Collider[] colliders,
                                                int count,
                                                MeleeAttackEvent attack,
                                                float viewAngle)
    {
        int suitableCount = 0;
        for (int i = 0; i < count; i++)
        {
            var toTargetDir = (colliders[i].transform.position - attack.position).normalized;
            var angleToTarget = Vector3.Angle(attack.direction, toTargetDir);
            if (angleToTarget > viewAngle / 2)
                continue;

            if (!IsValidView(world, colliders[i]))
                continue;

            var temp = colliders[suitableCount];
            colliders[suitableCount] = colliders[i];
            colliders[i] = temp;
            suitableCount++;
        }

        return suitableCount;
    }

    private static void SortCollidersByDistance(Collider[] colliders, Vector3 to, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var iInversed = count - 1 - i;
            for (int j = 0; j < count; j++)
            {
                var sqDistance1 = (colliders[i].transform.position - to).sqrMagnitude;
                var sqDistance2 = (colliders[j].transform.position - to).sqrMagnitude;
                if (sqDistance1 < sqDistance2)
                {
                    var temp = colliders[j];
                    colliders[j] = colliders[i];
                    colliders[i] = temp;
                }

                var jInversed = count - 1 - j;
                var sqDistance3 = (colliders[iInversed].transform.position - to).sqrMagnitude;
                var sqDistance4 = (colliders[jInversed].transform.position - to).sqrMagnitude;
                if (sqDistance3 > sqDistance4)
                {
                    var temp = colliders[jInversed];
                    colliders[jInversed] = colliders[iInversed];
                    colliders[iInversed] = temp;
                }
            }
        }
    }
}