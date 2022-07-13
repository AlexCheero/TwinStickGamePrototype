using Components;
using ECS;
using Tags;
using UnityEngine;

public class EnemyMeleeAttackSystem : EcsSystem
{
    private int _enemyFilterId;

    public EnemyMeleeAttackSystem(EcsWorld world)
    {
        _enemyFilterId = world.RegisterFilter(
            new BitMask(
                Id<EnemyTag>(),
                Id<Transform>(),
                Id<ReachComponent>(),
                Id<DamageComponent>(),
                Id<AttackCooldown>(),
                Id<ViewAngle>(),
                Id<TargetEntityComponent>()
                ));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_enemyFilterId))
        {
            var targetView = world.GetComponent<TargetEntityComponent>(id).target;
            
            var transform = world.GetComponent<Transform>(id);
            var position = transform.position;

            var playerFwd = transform.forward;
            var targetPos = targetView.transform.position;
            var toTargetDir = (targetPos - position).normalized;
            var angleToTarget = Vector3.Angle(playerFwd, toTargetDir);
            var viewAngle = world.GetComponent<ViewAngle>(id).angle;
            if (angleToTarget > viewAngle / 2)
                continue;

            var attackReach = world.GetComponent<ReachComponent>(id).distance;
            var distance = (targetPos - position).magnitude;
            if (distance > attackReach)
                continue;

            ref var attackCD = ref world.GetComponentByRef<AttackCooldown>(id);
            var nextAttackTime = attackCD.previousAttackTime + attackCD.attackCD;
            if (Time.time < nextAttackTime)
                continue;
            attackCD.previousAttackTime = Time.time;

            if (!Physics.Raycast(position, targetPos - position, out RaycastHit hit, attackReach))
                continue;

            var hitColliderView = hit.collider.gameObject.GetComponent<EntityView>();
            if (hitColliderView == null)
                continue;

            if (hitColliderView.Id != targetView.Id)
                continue;

            var damage = world.GetComponent<DamageComponent>(id).damage;
            world.GetComponentByRef<HealthComponent>(targetView.Id).health -= damage;
        }
    }
}
