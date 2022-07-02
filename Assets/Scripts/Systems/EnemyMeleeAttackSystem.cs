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
                Id<MeleeWeaponHoldingTag>(),
                Id<Transform>(),
                Id<ReachComponent>(),
                Id<DamageComponent>(),
                Id<AttackComponent>(),
                Id<ViewAngle>(),
                Id<TargetEntityComponent>()
                ));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_enemyFilterId))
        {
            var targetTransform = world.GetComponent<TargetEntityComponent>(id).target;
            //TODO: probably GameObject.GetComponent<> is too expensive for update
            //      and I should change TargetTransformComponent to TargetEntityComponent
            var targetView = targetTransform.gameObject.GetComponent<EntityView>();
            if (targetView == null)
                continue;

            var transform = world.GetComponent<Transform>(id);
            var position = world.GetComponent<Transform>(id).position;

            var playerFwd = transform.forward;
            var targetPos = targetTransform.transform.position;
            var toTargetDir = (targetPos - position).normalized;
            var angleToTarget = Vector3.Angle(playerFwd, toTargetDir);
            var viewAngle = world.GetComponent<ViewAngle>(id).angle;
            if (angleToTarget > viewAngle / 2)
                continue;

            var attackReach = world.GetComponent<ReachComponent>(id).distance;
            var distance = (targetPos - position).magnitude;
            if (distance > attackReach)
                continue;

            if (!Physics.Raycast(position, targetPos - position, out RaycastHit hit, attackReach))
                continue;

            var hitColliderView = hit.collider.gameObject.GetComponent<EntityView>();
            if (hitColliderView == null)
                continue;


            if (hitColliderView.Id != targetView.Id)
                continue;

            ref var attackComponent = ref world.GetComponent<AttackComponent>(id);
            var nextAttackTime = attackComponent.previousAttackTime + attackComponent.attackCD;
            if (Time.time < nextAttackTime)
                continue;

            Debug.Log("Enemy attack!");
            var damage = world.GetComponent<DamageComponent>(id).damage;
            world.GetComponent<HealthComponent>(targetView.Id).health -= damage;
            attackComponent.previousAttackTime = Time.time;
        }
    }
}
