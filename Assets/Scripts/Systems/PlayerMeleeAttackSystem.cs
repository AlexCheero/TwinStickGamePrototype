using Components;
using ECS;
using Tags;
using UnityEngine;

public class PlayerMeleeAttackSystem : EcsSystem
{
    private int _filterId;

    private const int OverlapsCount = 16;
    private Collider[] _overlapResults;

    public PlayerMeleeAttackSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(
            new BitMask(
                Id<PlayerTag>(),
                Id<MeleeWeaponHoldingTag>(),
                Id<Transform>(),
                Id<ReachComponent>(),
                Id<DamageComponent>(),
                Id<AttackComponent>()
                ));

        _overlapResults = new Collider[OverlapsCount];
    }

    public override void Tick(EcsWorld world)
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        foreach (var id in world.Enumerate(_filterId))
        {
            Debug.Log("Player melee attack!");

            ref var attackComponent = ref world.GetComponent<AttackComponent>(id);
            var nextAttackTime = attackComponent.previousAttackTime + attackComponent.attackCD;
            if (Time.time < nextAttackTime)
                continue;

            var transform = world.GetComponent<Transform>(id);
            var attackDistance = world.GetComponent<ReachComponent>(id).distance;
            var position = transform.position;
            //TODO: set masks for physics
            var overlapCount = Physics.OverlapSphereNonAlloc(position, attackDistance * 1000, _overlapResults);
            if (overlapCount <= 0)
                continue;

            //TODO: choose what base type could be added as component in inspector
            //var playersCollider = world.GetComponent<Collider>(id);
            var playersCollider = transform.gameObject.GetComponent<Collider>();
            overlapCount = MoveAllSuitableCollidersToFront(world, _overlapResults, overlapCount, playersCollider);
            if (overlapCount <= 0)
                continue;

            SortCollidersByDistance(_overlapResults, position, overlapCount);

            for (int i = 0; i < overlapCount; i++)
            {
                var targetPos = _overlapResults[i].transform.position;
                if (Physics.Raycast(position, targetPos - position, out RaycastHit hit, attackDistance) &&
                    hit.collider == _overlapResults[i])
                {
                    Debug.Log("Have hit!");
                    break;
                }
            }
        }
    }

    private bool IsValidView(EcsWorld world, Collider collider)
    {
        var targetView = collider.gameObject.GetComponent<EntityView>();
        if (targetView == null)
            return false;

        var targetEntity = targetView.Entity;
        if (!world.IsEntityValid(targetEntity))
            return false;

        if (!world.Have<HealthComponent>(targetEntity.GetId()))
            return false;

        return true;
    }

    private int MoveAllSuitableCollidersToFront(EcsWorld world, Collider[] colliders, int count, Collider except)
    {
        int suitableCount = 0;
        for (int i = 0; i < count; i++)
        {
            //TODO: add angle check
            if (!IsValidView(world, colliders[i]) || colliders[i] == except)
                continue;

            var temp = colliders[suitableCount];
            colliders[suitableCount] = colliders[i];
            colliders[i] = temp;
            suitableCount++;
        }

        return suitableCount;
    }

    private void SortCollidersByDistance(Collider[] colliders, Vector3 to, int count)
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
