using Components;
using ECS;
using Tags;
using UnityEngine;

public class EnemyMeleeAttackSystem : EcsSystem
{
    private int _playerFilterId;
    private int _enemyFilterId;

    public EnemyMeleeAttackSystem(EcsWorld world)
    {
        _playerFilterId = world.RegisterFilter(new BitMask(Id<PlayerTag>(), Id<Transform>(), Id<HealthComponent>()));
        _enemyFilterId = world.RegisterFilter(
            new BitMask(
                Id<EnemyTag>(),
                Id<MeleeWeaponHoldingTag>(),
                Id<Transform>(),
                Id<ReachComponent>(),
                Id<DamageComponent>(),
                Id<AttackComponent>(),
                Id<ViewAngle>()
                ));
    }

    public override void Tick(EcsWorld world)
    {
        int playerEntity = -1;
        foreach (var id in world.Enumerate(_playerFilterId))
        {
            playerEntity = id;
            break;
        }

        if (playerEntity < 0)
            return;

        var targetPos = world.GetComponent<Transform>(playerEntity).position;

        foreach (var id in world.Enumerate(_enemyFilterId))
        {
            var transform = world.GetComponent<Transform>(id);
            var position = world.GetComponent<Transform>(id).position;

            var playerFwd = transform.forward;
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

            if (hitColliderView.Id != playerEntity)
                continue;

            ref var attackComponent = ref world.GetComponent<AttackComponent>(id);
            var nextAttackTime = attackComponent.previousAttackTime + attackComponent.attackCD;
            if (Time.time < nextAttackTime)
                continue;

            Debug.Log("Enemy attack!");
            var damage = world.GetComponent<DamageComponent>(id).damage;
            world.GetComponent<HealthComponent>(playerEntity).health -= damage;
            attackComponent.previousAttackTime = Time.time;
        }
    }
}
