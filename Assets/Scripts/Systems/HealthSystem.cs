using Components;
using ECS;
using System.Collections.Generic;
using UnityEngine;

public class HealthSystem : EcsSystem
{
    private int _filterId;

    public HealthSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<HealthComponent>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var entity in world.Enumerate(_filterId))
        {
            var health = world.GetComponent<HealthComponent>(entity).health;
            if (health > 0)
                continue;

            if (world.Have<Transform>(entity))
                Object.Destroy(world.GetComponent<Transform>(entity).gameObject);
            world.Delete(entity);
        }
    }
}
