using ECS;
using UnityEngine;

public class PlayerMeleeAttackSystem : EcsSystem
{
    private int _filterId;

    public PlayerMeleeAttackSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(
            new BitMask(
                Id<PlayerTag>(),
                Id<Transform>(),
                Id<MeleeAttackReachComponent>(),
                Id<MeleeDamageComponent>(),
                Id<MeleeAttackComponent>()
                ));
    }

    public override void Tick(EcsWorld world)
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        Debug.Log("Player attack!");

        world.GetFilter(_filterId).Iterate((entities, count) =>
        {
            for (int i = 0; i < count; i++)
            {
                ref var attackComponent = ref world.GetComponent<MeleeAttackComponent>(entities[i]);
                var nextAttackTime = attackComponent.previousAttackTime + attackComponent.attackCD;
                if (Time.time < nextAttackTime)
                    continue;

                var transform = world.GetComponent<Transform>(entities[i]);
                Ray ray = new Ray(transform.position, transform.forward);
                var attackDistance = world.GetComponent<MeleeAttackReachComponent>(entities[i]).distance;
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

                Debug.Log("Player hit!");
                world.GetComponent<HealthComponent>(targetEntityId).health -=
                    world.GetComponent<MeleeDamageComponent>(entities[i]).damage;

                attackComponent.previousAttackTime = Time.time;
            }
        });
    }
}
