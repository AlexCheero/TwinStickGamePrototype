using Components;
using ECS;
using Tags;
using UnityEngine;

public class PlayerMeleeAttackSystem : EcsSystem
{
    private int _filterId;
    private Collider[] _physResults;

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

        _physResults = new Collider[4];
    }

    public override void Tick(EcsWorld world)
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        foreach (var id in world.Enumerate(_filterId))
        {
            Debug.Log("Player melee attack!");

            ref var attackComponent = ref world.GetComponent<AttackComponent>(id);
            var nextAttackTime = attackComponent.previousAttackTime + attackComponent.attackCD;
            if (Time.time < nextAttackTime)
                continue;

            var transform = world.GetComponent<Transform>(id);
            var attackDistance = world.GetComponent<ReachComponent>(id).distance;
            var overlapCount = Physics.OverlapSphereNonAlloc(transform.position, attackDistance, _physResults);
            if (overlapCount <= 0)
                continue;

            for (int i = 0; i < overlapCount; i++)
            {
                Vector3 targetPos;
                if (IsEntityViewWithHealth(world, _physResults[i], id, out targetPos) < 0)
                    continue;

                var ray = new Ray(transform.position, (targetPos - transform.position).normalized);
                if (!Physics.Raycast(ray, out RaycastHit hit, attackDistance))
                    continue;

                var targetId = IsEntityViewWithHealth(world, hit.collider, id, out targetPos);
                if (targetId < 0)
                    continue;

                Debug.Log("Player melee hit!");
                world.GetComponent<HealthComponent>(targetId).health -=
                    world.GetComponent<DamageComponent>(id).damage;

                attackComponent.previousAttackTime = Time.time;
                
                //TODO: implement cw/ccw sort for side attacks and min angle with fwd for straight attacks
                //      or loop through all for area attacks
                break;
            }
        }
    }

    private int IsEntityViewWithHealth(EcsWorld world, Collider collider, int attackerId, out Vector3 position)
    {
        position = Vector3.zero;
        var targetView = collider.gameObject.GetComponent<EntityView>();
        if (targetView == null)
            return -1;

        var targetEntity = targetView.Entity;
        if (!world.IsEntityValid(targetEntity))
            return -1;

        var targetId = targetEntity.GetId();
        if (!world.Have<HealthComponent>(targetId))
            return -1;

        position = targetView.transform.position;
        return targetId;
    }
}
