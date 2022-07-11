using Components;
using ECS;
using Tags;
using UnityEngine;

public class PlayerInstantRangedAttackSystem : EcsSystem
{
    private int _filterId;

    public PlayerInstantRangedAttackSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(
            new BitMask(
                Id<PlayerTag>(),
                Id<InstantRangedWeaponHoldingTag>(),
                Id<Transform>(),
                Id<DamageComponent>(),
                Id<AttackComponent>(),
                Id<Ammo>()
                ));
    }

    public override void Tick(EcsWorld world)
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        foreach (var id in world.Enumerate(_filterId))
        {
            ref var attackComponent = ref world.GetComponentByRef<AttackComponent>(id);
            var nextAttackTime = attackComponent.previousAttackTime + attackComponent.attackCD;
            if (Time.time < nextAttackTime)
                continue;
            attackComponent.previousAttackTime = Time.time;

#if DEBUG
            if (world.GetComponent<Ammo>(id).amount <= 0)
                throw new System.Exception("ammo amount is <= 0. have ammo component: " + world.Have<Ammo>(id));
#endif

            var newAmmo = world.GetComponent<Ammo>(id);
            newAmmo.amount--;
            world.SetComponent(id, newAmmo);

            Debug.Log("Player instant ranged attack!");

            var transform = world.GetComponent<Transform>(id);
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;
            //TODO: use non alloc version everywhere where Raycast is used
            //but make sure that non alloc version can catch the nearest object
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

            Debug.Log("Player instant ranged hit!");
            world.GetComponentByRef<HealthComponent>(targetEntityId).health -=
                world.GetComponent<DamageComponent>(id).damage;
        }
    }
}
