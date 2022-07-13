using Components;
using ECS;
using Tags;
using UnityEngine;

public class EnemyMeleeAttackSystem : EcsSystem
{
    private int _enemyFilterId;

    public EnemyMeleeAttackSystem(EcsWorld world)
    {
        _enemyFilterId = world.RegisterFilter(
            new BitMask(
                Id<EnemyTag>(),
                Id<Transform>(),
                Id<ReachComponent>(),
                Id<ViewAngle>(),
                Id<TargetEntityComponent>()
                ));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_enemyFilterId))
        {
            var targetView = world.GetComponent<TargetEntityComponent>(id).target;
            
            var transform = world.GetComponent<Transform>(id);
            var position = transform.position;

            var playerFwd = transform.forward;
            var targetPos = targetView.transform.position;
            var toTargetDir = (targetPos - position).normalized;
            var angleToTarget = Vector3.Angle(playerFwd, toTargetDir);
            var viewAngle = world.GetComponent<ViewAngle>(id).angle;
            if (angleToTarget > viewAngle / 2)
                continue;

            var attackReach = world.GetComponent<ReachComponent>(id).distance;
            var distance = (targetPos - position).magnitude;
            if (distance > attackReach)
                continue;

            var weaponEntity = world.GetComponentByRef<CurrentWeapon>(id).entity;
#if DEBUG
            if (!world.IsEntityValid(weaponEntity))
                throw new System.Exception("invalid weapon entity");
#endif
            var weaponId = weaponEntity.GetId();
#if DEBUG
            if (world.Have<Attack>(weaponId))
                throw new System.Exception("please clean Attack component from weapon");
#endif
            world.Add(weaponId, new Attack { position = transform.position, direction = transform.forward });
        }
    }
}
