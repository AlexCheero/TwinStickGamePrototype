using Components;
using ECS;
using Tags;
using UnityEngine;

//choose system type here
[System(ESystemCategory.Update)]
public class EnemyVisionSystem : EcsSystem
{
    private int _filterId;

    public EnemyVisionSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(
            new BitMask(
                Id<EnemyTag>(),
                Id<Transform>(),
                Id<ViewAngle>(),
                Id<ViewDistance>(),
                Id<TargetEntityComponent>()
                ),
            new BitMask(Id<SeenEnemyTag>())
            );
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            var transform = world.GetComponent<Transform>(id);
            var position = transform.position;
            var targetPosition = world.GetComponent<TargetEntityComponent>(id).target.transform.position;
            var sqrDistance = (targetPosition - position).sqrMagnitude;
            var viewDistance = world.GetComponent<ViewDistance>(id).distance;
            if (sqrDistance > viewDistance * viewDistance)
                continue;

            var toTargetDir = (targetPosition - position).normalized;
            var angleToTarget = Vector3.Angle(transform.forward, toTargetDir);
            var viewAngle = world.GetComponent<ViewAngle>(id).angle;
            if (angleToTarget <= viewAngle / 2)
                world.Add<SeenEnemyTag>(id);
        }
    }
}