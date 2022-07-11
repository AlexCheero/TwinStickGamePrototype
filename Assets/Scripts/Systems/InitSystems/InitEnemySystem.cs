using Components;
using ECS;
using Tags;
using UnityEngine.AI;

[InitSystem]
public class InitEnemySystem : EcsSystem
{
    private int _filterId;

    public InitEnemySystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<EnemyTag>(), Id<NavMeshAgent>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            var navAgent = world.GetComponent<NavMeshAgent>(id);
            navAgent.speed = world.GetComponent<SpeedComponent>(id).speed;
            navAgent.stoppingDistance = world.GetComponent<ReachComponent>(id).distance;
            navAgent.angularSpeed = world.GetComponent<AngularSpeedComponent>(id).speed;
            navAgent.acceleration = world.GetComponent<AccelerationComponent>(id).acceleration;
        }
    }
}
