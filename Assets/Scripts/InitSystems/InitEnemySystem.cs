using Components;
using ECS;
using Tags;
using UnityEngine.AI;

public class InitEnemySystem : EcsSystem
{
    private int _filterId;

    public InitEnemySystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<EnemyTag>(), Id<NavMeshAgent>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var enemyEntity in world.Enumerate(_filterId))
        {
            var navAgent = world.GetComponent<NavMeshAgent>(enemyEntity);
            navAgent.speed = world.GetComponent<SpeedComponent>(enemyEntity).speed;
            navAgent.stoppingDistance = world.GetComponent<ReachComponent>(enemyEntity).distance;
            navAgent.angularSpeed = world.GetComponent<AngularSpeedComponent>(enemyEntity).speed;
            navAgent.acceleration = world.GetComponent<AccelerationComponent>(enemyEntity).acceleration;
        }
    }
}
