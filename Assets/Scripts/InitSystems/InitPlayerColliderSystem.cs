using ECS;
using System.Collections;
using System.Collections.Generic;
using Tags;
using UnityEngine;

[InitSystem]
public class InitPlayerColliderSystem : EcsSystem
{
    private int _filterId;

    public InitPlayerColliderSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<PlayerTag>(), Id<Transform>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            var go = world.GetComponent<Transform>(id).gameObject;
            var collider = go.GetComponent<Collider>();
            world.AddComponent(id, collider);
        }
    }
}
