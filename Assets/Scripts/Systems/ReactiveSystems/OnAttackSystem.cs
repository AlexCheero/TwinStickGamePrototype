using Components;
using ECS;
using Tags;
using UnityEngine;

[ReactiveSystem(EReactionType.OnAdd, typeof(Attack))]
public static class OnAttackSystem
{
    private static BitMask _meleeIncludes = new BitMask(ComponentMeta<MeleeWeapon>.Id,
                                                        ComponentMeta<DamageComponent>.Id,
                                                        ComponentMeta<ReachComponent>.Id,
                                                        ComponentMeta<AttackAngle>.Id,
                                                        ComponentMeta<AttackCooldown>.Id);

    private static BitMask _rangedIncludes = new BitMask(ComponentMeta<RangedWeapon>.Id,
                                                         ComponentMeta<Ammo>.Id,
                                                         ComponentMeta<DamageComponent>.Id,
                                                         ComponentMeta<AttackCooldown>.Id);

    private static BitMask _projIncludes = new BitMask(ComponentMeta<ProjectileWeapon>.Id,
                                                       ComponentMeta<Ammo>.Id,
                                                       ComponentMeta<AttackCooldown>.Id);

    

    private static ObjectPool _projectilePool;
    private const int OverlapsCount = 16;
    private static Collider[] _overlapResults;

    public static void Tick(EcsWorld world, int id)
    {
        //TODO: move to update system and init this values in ctor
        if (_projectilePool == null)
            _projectilePool = GameObject.Find("ProjectilePool").GetComponent<ObjectPool>();
        if (_overlapResults == null)
            _overlapResults = new Collider[OverlapsCount];

        ref var attackCD = ref world.GetComponentByRef<AttackCooldown>(id);
        var nextAttackTime = attackCD.previousAttackTime + attackCD.attackCD;
        if (Time.time < nextAttackTime)
            return;

        attackCD.previousAttackTime = Time.time;

        if (world.CheckAgainstMasks(id, _meleeIncludes))
            MeleeAttack(world, id);
        else if(world.CheckAgainstMasks(id, _rangedIncludes))
            RangedAttack(world, id);
        else if (world.CheckAgainstMasks(id, _projIncludes))
            ProjectileAttack(world, id);

        world.RemoveComponent<Attack>(id);
    }

    private static void MeleeAttack(EcsWorld world, int id)
    {
        Debug.Log("melee attack!");

        var attackDistance = world.GetComponent<ReachComponent>(id).distance;
        var attack = world.GetComponent<Attack>(id);
        var position = attack.position;
        //TODO: set masks for physics
        var overlapCount = Physics.OverlapSphereNonAlloc(position, attackDistance, _overlapResults);
        if (overlapCount <= 0)
            return;

        var angle = world.GetComponent<AttackAngle>(id).angle;
        overlapCount = MoveAllSuitableCollidersToFront(world, _overlapResults, overlapCount, attack, angle);
        if (overlapCount <= 0)
            return;

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
                                                Attack attack,
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

    private static void RangedAttack(EcsWorld world, int id)
    {
#if DEBUG
        if (world.GetComponent<Ammo>(id).amount <= 0)
            throw new System.Exception("ammo amount is <= 0. have ammo component: " + world.Have<Ammo>(id));
#endif

        var newAmmo = world.GetComponent<Ammo>(id);
        newAmmo.amount--;
        world.SetComponent(id, newAmmo);

        Debug.Log("instant ranged attack!");

        var attack = world.GetComponent<Attack>(id);
        Ray ray = new Ray(attack.position, attack.direction);
        RaycastHit hit;
        if (!Physics.Raycast(ray, out hit))
            return;

        var targetView = hit.collider.gameObject.GetComponent<EntityView>();
        if (targetView == null)
            return;

        var targetEntity = targetView.Entity;
        if (!world.IsEntityValid(targetEntity))
            return;

        var targetEntityId = targetEntity.GetId();
        if (!world.Have<HealthComponent>(targetEntityId) || world.Have<Pickup>(targetEntityId))
            return;

        Debug.Log("instant ranged hit!");
        world.GetComponentByRef<HealthComponent>(targetEntityId).health -=
            world.GetComponent<DamageComponent>(id).damage;
    }

    private static void ProjectileAttack(EcsWorld world, int id)
    {
#if DEBUG
        if (world.GetComponent<Ammo>(id).amount <= 0)
            throw new System.Exception("OnProjectileShotSystem. ammo amount is <= 0. have ammo component: " + world.Have<Ammo>(id));
#endif

        var attack = world.GetComponent<Attack>(id);

        var instantiationPosition = attack.position + attack.direction * 2.0f; //instantiation before the player
        var projectileView = _projectilePool.Get<EntityView>(instantiationPosition, Quaternion.identity);
        var projectileId = projectileView.InitAsEntity(world);
#if DEBUG
        if (!world.Have<Projectile>(projectileId))
            throw new System.Exception("projectileView have no Projectile tag");
#endif
        var speed = world.GetComponent<SpeedComponent>(projectileId).speed;
        projectileView.GetComponent<Rigidbody>().AddForce(attack.direction * speed);//TODO: try different force types

        var newAmmo = world.GetComponent<Ammo>(id);
        newAmmo.amount--;
        world.SetComponent(id, newAmmo);
    }
}
