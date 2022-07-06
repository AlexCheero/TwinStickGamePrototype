using Components;
using ECS;
using Tags;
using UnityEngine;

public class HealthSystem : EcsSystem
{
    private int _filterId;

    public HealthSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<HealthComponent>()));
    }

#if DEBUG
    float hlth;
#endif

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            var health = world.GetComponent<HealthComponent>(id).health;

#if DEBUG
            //TODO: remove this code when I get proper UI
            if (world.Have<PlayerTag>(id) && hlth != health)
            {
                Debug.Log("Player's health: " + health);
                hlth = health;
            }
#endif

            if (health > 0)
                continue;

            world.AddTag<DeadTag>(id);
        }
    }
}
