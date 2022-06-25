using ECS;
using UnityEngine;
using UnityEngine.AI;

public class EnemyFollowSystem : EcsSystem
{
    private int _enemyFilterId;

    public EnemyFollowSystem(EcsWorld world)
    {
        _enemyFilterId = world.RegisterFilter(
            new BitMask(
                Id<EnemyTag>(),
                Id<NavMeshAgent>(),
                Id<TargetTransformComponent>(),
                Id<Transform>()
                ));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var entity in world.Enumerate(_enemyFilterId))
        {
            var navAgent = world.GetComponent<NavMeshAgent>(entity);
            var target = world.GetComponent<TargetTransformComponent>(entity).target;
            if (target == null)
            {
                world.RemoveComponent<TargetTransformComponent>(entity);
                continue;
            }

            const float sqrMargin = 0.1f;
            if ((navAgent.destination - target.position).sqrMagnitude > sqrMargin)
            {
                navAgent.SetDestination(target.position);
            }
            
            var transform = world.GetComponent<Transform>(entity);
            var direction = (target.position - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}
