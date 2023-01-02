using Components;
using ECS;
using Tags;
using UnityEngine;
using UnityEngine.AI;

[System(ESystemCategory.Update)]
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
                Id<SeenEnemyTag>()
                ));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_enemyFilterId))
        {
            var navAgent = world.GetComponent<NavMeshAgent>(id);
            var targetPostion = world.GetComponent<TargetEntityComponent>(id).target.transform.position;

            const float sqrMargin = 0.1f;
            if ((navAgent.destination - targetPostion).sqrMagnitude > sqrMargin)
                navAgent.SetDestination(targetPostion);
        }
    }
}
