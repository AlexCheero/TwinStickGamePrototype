using ECS;
using UnityEngine;
using UnityEngine.AI;

//choose system type here
[System(ESystemCategory.Update)]
public class BotMovementAnimationSystem : EcsSystem
{
    private int _filterId;

    public BotMovementAnimationSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<NavMeshAgent>(), Id<Animator>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            var navAgent = world.GetComponent<NavMeshAgent>(id);
            var animator = world.GetComponent<Animator>(id);

            bool isMoving = navAgent.velocity.sqrMagnitude > 0.01f;
            animator.SetFloat("Vertical", isMoving ? 1 : 0);
        }
    }
}