using Components;
using ECS;
using Tags;
using UnityEngine;

public class PlayerMeleeAttackSystem : EcsSystem
{
    private int _filterId;

    public PlayerMeleeAttackSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(
            new BitMask(
                Id<PlayerTag>(),
                Id<MeleeWeaponHoldingTag>(),
                Id<Transform>(),
                Id<ReachComponent>(),
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
            Debug.Log("Player melee attack!");

            ref var attackComponent = ref world.GetComponent<AttackComponent>(entity);
            var nextAttackTime = attackComponent.previousAttackTime + attackComponent.attackCD;
            if (Time.time < nextAttackTime)
                continue;

            var transform = world.GetComponent<Transform>(entity);
            Ray ray = new Ray(transform.position, transform.forward);
            var attackDistance = world.GetComponent<ReachComponent>(entity).distance;
            RaycastHit hit;
            if (!Physics.Raycast(ray, out hit, attackDistance))
                continue;

            var targetView = hit.collider.gameObject.GetComponent<EntityView>();
            if (targetView == null)
                continue;

            var targetEntity = targetView.Entity;
            if (!world.IsEntityValid(targetEntity))
                continue;

            var targetEntityId = targetEntity.GetId();
            if (!world.Have<HealthComponent>(targetEntity.GetId()))
                continue;

            Debug.Log("Player melee hit!");
            world.GetComponent<HealthComponent>(targetEntityId).health -=
                world.GetComponent<DamageComponent>(entity).damage;

            attackComponent.previousAttackTime = Time.time;
        }
    }
}
