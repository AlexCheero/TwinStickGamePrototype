using Components;
using ECS;
using UnityEngine;

//choose system type here
[System(ESystemCategory.Update)]
public class ApplyDamageSystem : EcsSystem
{
    private readonly int _filterId;

    public ApplyDamageSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<HealthComponent>(), Id<DamageComponent>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            world.GetComponentByRef<HealthComponent>(id).health -=
                world.GetComponent<DamageComponent>(id).damage;
            world.Remove<DamageComponent>(id);

            if (world.Have<Transform>(id))
            {
                var ps = GameObject.Instantiate(Resources.Load<ParticleSystem>("ParticleSystems/Blood/Blood_PS"));
                var pos = world.GetComponent<Transform>(id).position;
                pos.y += 1.5f;
                ps.transform.position = pos;
            }
        }
    }
}