using Components;
using ECS;
using Tags;
using UnityEngine;

//choose system type here
[System(ESystemCategory.Update)]
public class MovementAnimationSystem : EcsSystem
{
    private int _filterId;

    public MovementAnimationSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<PlayerTag>(), Id<PlayerVelocityComponent>(), Id<Animator>(), Id<Transform>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            var transform = world.GetComponent<Transform>(id);
            var animator = world.GetComponent<Animator>(id);
            var moveDirection = world.GetComponent<PlayerVelocityComponent>(id).velocity.normalized;

            var verticalSign = Mathf.Sign(Vector3.Dot(moveDirection, transform.forward));
            var vertical = verticalSign * Vector3.Project(moveDirection, transform.forward).magnitude;
            var horizontalSign = Mathf.Sign(Vector3.Dot(moveDirection, transform.right));
            var horizontal = horizontalSign * Vector3.Project(moveDirection, transform.right).magnitude;

            animator.SetFloat("Vertical", vertical);
            animator.SetFloat("Horizontal", horizontal);
        }
    }
}