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
                                                     Id<DamageComponent>(),
                                                     Id<AttackCooldown>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
#if DEBUG
            if (world.GetComponent<Ammo>(id).amount <= 0)
                throw new System.Exception("ammo amount is <= 0. have ammo component: " + world.Have<Ammo>(id));
#endif

            world.GetComponentByRef<Ammo>(id).amount--;

            Debug.Log("instant ranged attack!");

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