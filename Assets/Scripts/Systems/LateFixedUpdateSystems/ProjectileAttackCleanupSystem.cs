using Components;
using ECS;
using Tags;
using UnityEngine;

//choose system type here
[System(ESystemCategory.LateFixedUpdate)]
public class ProjectileAttackCleanupSystem : EcsSystem
{
    private int _filterId;

    public ProjectileAttackCleanupSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<Attack>(), Id<ProjectileWeapon>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
            world.Remove<Attack>(id);
    }
}