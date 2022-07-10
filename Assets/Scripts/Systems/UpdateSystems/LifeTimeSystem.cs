using Components;
using ECS;
using Tags;
using UnityEngine;

[UpdateSystem]
public class LifeTimeSystem : EcsSystem
{
    private int _filterId;

    public LifeTimeSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<LifeTime>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            ref var time = ref world.GetComponent<LifeTime>(id).time;
            time -= Time.deltaTime;
            if (time <= 0)
                world.Add<DeadTag>(id);
        }
    }
}