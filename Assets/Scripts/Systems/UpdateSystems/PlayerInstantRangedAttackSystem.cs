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
                Id<AttackComponent>()
                ));
    }

    public override void Tick(EcsWorld world)
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        foreach (var entity in world.Enumerate(_filterId))
        {
            Debug.Log("Player instant ranged attack!");

            ref var attackComponent = ref world.GetComponentByRef<AttackComponent>(entity);
            var nextAttackTime = attackComponent.previousAttackTime + attackComponent.attackCD;
            if (Time.time < nextAttackTime)
                continue;
            attackComponent.previousAttackTime = Time.time;

            var transform = world.GetComponent<Transform>(entity);
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
                world.GetComponent<DamageComponent>(entity).damage;
        }
    }
}
