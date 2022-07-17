using Components;
using ECS;
using Tags;
using UnityEngine.AI;

//choose system type here
[System(ESystemCategory.Init)]
public class StartPatrolSystem : EcsSystem
{
    private int _filterId;

    public StartPatrolSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(
            new BitMask(
                Id<EnemyTag>(),
                Id<NextWaypointIdx>(),
                Id<NavMeshAgent>())
            );

        WaypointsManager.Gather();
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            var navAgent = world.GetComponent<NavMeshAgent>(id);
            navAgent.SetDestination(WaypointsManager.WaypointPositions[0]);
        }
    }
}