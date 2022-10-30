using Components;
using ECS;
using Tags;
using UnityEngine;

//choose system type here
[System(ESystemCategory.Update)]
public class ApplyDamageSystem : EcsSystem
{
    private readonly int _filterId;

    public ApplyDamageSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(
            Id<HealthComponent>(),
            Id<ImpactEffect>(),
            Id<DamageComponent>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            world.GetComponentByRef<HealthComponent>(id).health -=
                world.GetComponent<DamageComponent>(id).damage;
            world.Remove<DamageComponent>(id);

            if (world.Have<Impact>(id) && world.Have<Transform>(id))
            {
                var effectPosition = world.GetComponent<Impact>(id).position;
                var effectPoolName = world.GetComponent<ImpactEffect>(id).poolName;
                var effectObject = PoolManager.Get(effectPoolName).Get(effectPosition);
                var particleSystem = effectObject.GetComponent<ParticleSystem>();
                particleSystem.transform.SetParent(world.GetComponent<Transform>(id), true);

                effectObject.GetComponent<EntityView>().InitAsEntity(world);

                world.Remove<Impact>(id);
            }
        }
    }
}