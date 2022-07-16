using Components;
using ECS;

[System(ESystemCategory.Update)]
public class AttackCleanupSystem : EcsSystem
{
    private int _filterId;

    public AttackCleanupSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<Attack>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            world.Remove<Attack>(id);
        }
    }
}