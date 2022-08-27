using Components;
using ECS;
using Tags;
using UnityEngine;

[System(ESystemCategory.Update)]
public class EnemyAttackSystem : EcsSystem
{
    private int _filterId;

    public EnemyAttackSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<EnemyTag>(),
                                                          Id<Transform>(),
                                                          Id<AttackReachComponent>(),
                                                          Id<ViewAngle>(),
                                                          Id<CurrentWeapon>(),
                                                          Id<TargetEntityComponent>(),
                                                          Id<SeenEnemyTag>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
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

            var attackReach = world.GetComponent<AttackReachComponent>(id).distance;
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
