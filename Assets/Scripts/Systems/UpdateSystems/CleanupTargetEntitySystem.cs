using Components;
using ECS;

[System(ESystemCategory.Update)]
public class CleanupTargetEntitySystem : EcsSystem
{
    private int _filterId;

    public CleanupTargetEntitySystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<TargetEntityComponent>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            var target = world.GetComponent<TargetEntityComponent>(id).target;
            if (target == null || !world.IsEntityValid(target.Entity))
            {
                world.Remove<TargetEntityComponent>(id);
                continue;
            }
        }
    }
}
