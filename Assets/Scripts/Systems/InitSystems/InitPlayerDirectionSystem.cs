using Components;
using ECS;
using Tags;
using UnityEngine;

//choose system type here
[System(ESystemCategory.Init)]
public class InitPlayerDirectionSystem : EcsSystem
{
    private readonly int _filterId;

    public InitPlayerDirectionSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<PlayerTag>(), Id<PlayerDirectionComponent>(), Id<Transform>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
            world.GetComponent<PlayerDirectionComponent>(id).direction = world.GetComponent<Transform>(id).forward;
    }
}