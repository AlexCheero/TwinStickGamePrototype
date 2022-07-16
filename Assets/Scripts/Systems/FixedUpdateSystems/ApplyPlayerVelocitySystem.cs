using Components;
using ECS;
using Tags;
using UnityEngine;

//choose system type here
[System(ESystemCategory.FixedUpdate)]
public class ApplyPlayerVelocitySystem : EcsSystem
{
    private int _filterId;

    public ApplyPlayerVelocitySystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<PlayerTag>(), Id<PlayerVelocityComponent>(), Id<Rigidbody>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            var rigidBody = world.GetComponent<Rigidbody>(id);
            var velocity = world.GetComponent<PlayerVelocityComponent>(id).velocity;
            velocity.y = rigidBody.velocity.y;
            rigidBody.velocity = velocity;
        }
    }
}