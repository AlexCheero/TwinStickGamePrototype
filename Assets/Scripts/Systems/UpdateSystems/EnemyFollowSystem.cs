using Components;
using ECS;
using Tags;
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
                Id<TargetEntityComponent>(),
                Id<Transform>()
                ));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var entity in world.Enumerate(_enemyFilterId))
        {
            var navAgent = world.GetComponent<NavMeshAgent>(entity);
            var targetPostion = world.GetComponent<TargetEntityComponent>(entity).target.transform.position;

            const float sqrMargin = 0.1f;
            if ((navAgent.destination - targetPostion).sqrMagnitude > sqrMargin)
            {
                navAgent.SetDestination(targetPostion);
            }
            
            var transform = world.GetComponent<Transform>(entity);
            var direction = (targetPostion - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}
