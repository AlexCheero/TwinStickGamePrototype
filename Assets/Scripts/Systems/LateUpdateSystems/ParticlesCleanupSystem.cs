using ECS;
using Tags;
using UnityEngine;

//choose system type here
[System(ESystemCategory.LateUpdate)]
public class ParticlesCleanupSystem : EcsSystem
{
    private readonly int _filterId;

    public ParticlesCleanupSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<ParticleSystem>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            var ps = world.GetComponent<ParticleSystem>(id);
            if (ps.gameObject.activeSelf && !ps.IsAlive())
                world.Add<DeadTag>(id);
        }
    }
}