using Components;
using ECS;

//choose system type here
[System(ESystemCategory.LateFixedUpdate)]
public class CollisionCleanupSystem : EcsSystem
{
    private int _filterId;

    public CollisionCleanupSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<CollisionWith>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
            world.Remove<CollisionWith>(id);
    }
}