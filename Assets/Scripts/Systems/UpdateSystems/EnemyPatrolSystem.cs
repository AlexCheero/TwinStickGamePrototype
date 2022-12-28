using Components;
using ECS;
using Tags;
using UnityEngine;
using UnityEngine.AI;

//choose system type here
[System(ESystemCategory.Update)]
public class EnemyPatrolSystem : EcsSystem
{
    private int _filterId;

    public EnemyPatrolSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(
            new BitMask(
                Id<EnemyTag>(),
                Id<NextWaypointIdx>(),
                Id<Transform>(),
                Id<NavMeshAgent>())
            );
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            var position = world.GetComponent<Transform>(id).position;
            ref var nextWaypointIdx = ref world.GetComponent<NextWaypointIdx>(id).idx;
            var nextWaypointPos = WaypointsManager.WaypointPositions[nextWaypointIdx];
            var navAgent = world.GetComponent<NavMeshAgent>(id);
            var reach = navAgent.stoppingDistance;
            if ((position - nextWaypointPos).sqrMagnitude <= reach * reach)
            {
                nextWaypointIdx++;
                if (nextWaypointIdx >= WaypointsManager.WaypointPositions.Length)
                    nextWaypointIdx = 0;
                navAgent.SetDestination(WaypointsManager.WaypointPositions[nextWaypointIdx]);
            }    
        }
    }
}