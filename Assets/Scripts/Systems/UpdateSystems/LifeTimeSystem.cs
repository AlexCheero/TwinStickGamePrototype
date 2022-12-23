using Components;
using ECS;
using Tags;
using UnityEngine;

[System(ESystemCategory.Update)]
public class LifeTimeSystem : EcsSystem
{
    private int _filterId;
    private int _poolItemsfilterId;

    public LifeTimeSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<LifeTime>()), new BitMask(Id<PoolItem>()));
        _poolItemsfilterId = world.RegisterFilter(new BitMask(Id<LifeTime>(), Id<PoolItem>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            ref var time = ref world.GetComponentByRef<LifeTime>(id).time;
            time -= Time.deltaTime;
            if (time <= 0)
                world.Add<DeadTag>(id);
        }
        
        foreach (var id in world.Enumerate(_poolItemsfilterId))
        {
            var poolItem = world.GetComponent<PoolItem>(id);
            if (!poolItem.gameObject.activeSelf)
                continue;
            
            ref var time = ref world.GetComponentByRef<LifeTime>(id).time;
            time -= Time.deltaTime;
            if (time <= 0)
                world.Add<DeadTag>(id);
        }
    }
}