using Components;
using ECS;
using Tags;
using UnityEngine;

[System(ESystemCategory.Update)]
public class RangedAttackSystem : EcsSystem
{
    private int _filterId;

    public RangedAttackSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<Attack>(),
                                                     Id<RangedWeapon>(),
                                                     Id<Ammo>(),
                                                     Id<DamageComponent>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            ref var ammo = ref world.GetComponentByRef<Ammo>(id).amount;
            if (ammo == 0)
                continue;
#if DEBUG
            if (ammo < 0)
                throw new System.Exception("negative ammo");
            if (world.GetComponent<Ammo>(id).amount <= 0)
                throw new System.Exception("ammo amount is <= 0. have ammo component: " + world.Have<Ammo>(id));
#endif

            ammo--;

#if DEBUG
            if (!world.Have<Owner>(id))
                throw new System.Exception("weapon should have owner");
#endif
            var ownerId = world.GetComponent<Owner>(id).entity.GetId();
            if (world.Have<Animator>(ownerId))
                world.GetComponent<Animator>(ownerId).SetTrigger("IsFiring");

            var attack = world.GetComponent<Attack>(id);
            Ray ray = new Ray(attack.position, attack.direction);
            RaycastHit hit;
            if (!Physics.Raycast(ray, out hit))
                continue;

            var targetView = hit.collider.gameObject.GetComponent<EntityView>();
            if (targetView == null)
                continue;

            var targetEntity = targetView.Entity;
            if (!world.IsEntityValid(targetEntity))
                continue;

            var targetEntityId = targetEntity.GetId();
            if (!world.Have<HealthComponent>(targetEntityId) || world.Have<Pickup>(targetEntityId))
                continue;

            Debug.Log("instant ranged hit!");
            world.GetComponentByRef<HealthComponent>(targetEntityId).health -=
                world.GetComponent<DamageComponent>(id).damage;
        }
    }
}