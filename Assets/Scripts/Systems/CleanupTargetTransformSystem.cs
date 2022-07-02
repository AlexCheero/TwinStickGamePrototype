using Components;
using ECS;

public class CleanupTargetTransformSystem : EcsSystem
{
    private int _filterId;

    public CleanupTargetTransformSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<TargetTransformComponent>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            if (world.GetComponent<TargetTransformComponent>(id).target == null)
                world.RemoveComponent<TargetTransformComponent>(id);
        }
    }
}
