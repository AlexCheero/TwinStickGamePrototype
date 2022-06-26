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
                Id<AttackComponent>()
                ));
    }

    public override void Tick(EcsWorld world)
    {
        int playerEntity = -1;
        foreach (var entity in world.Enumerate(_playerFilterId))
        {
            playerEntity = entity;
            break;
        }

        if (playerEntity < 0)
            return;

        var targetTransform = world.GetComponent<Transform>(playerEntity);

        foreach (var entity in world.Enumerate(_enemyFilterId))
        {
            var transform = world.GetComponent<Transform>(entity);
            var attackReach = world.GetComponent<ReachComponent>(entity).distance;
            var distance = (targetTransform.position - transform.position).magnitude;
            if (distance > attackReach)
                continue;

            ref var attackComponent = ref world.GetComponent<AttackComponent>(entity);
            var nextAttackTime = attackComponent.previousAttackTime + attackComponent.attackCD;
            if (Time.time < nextAttackTime)
                continue;

            var damage = world.GetComponent<DamageComponent>(entity).damage;
            Debug.Log("Enemy attack!");
            world.GetComponent<HealthComponent>(playerEntity).health -= damage;
            attackComponent.previousAttackTime = Time.time;
        }
    }
}
