using Components;
using ECS;
using Tags;

public class HealthSystem : EcsSystem
{
    private int _filterId;

    public HealthSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<HealthComponent>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            var health = world.GetComponent<HealthComponent>(id).health;
            if (health > 0)
                continue;

            world.AddTag<DeadTag>(id);
        }
    }
}
