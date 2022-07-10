using ECS;
using Tags;
using UnityEngine;

[InitSystem]
public class InitPlayerColliderSystem : EcsSystem
{
    private int _filterId;

    public InitPlayerColliderSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<PlayerTag>(), Id<Transform>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            var go = world.GetComponent<Transform>(id).gameObject;
            //TODO: add ability to choose base class of UnityComponent when added to EntityView in custom inspector
            //      to be able to add Collider instead of CapsuleCollider to Player and delete this system
            var collider = go.GetComponent<Collider>();
            world.Add(id, collider);
        }
    }
}
