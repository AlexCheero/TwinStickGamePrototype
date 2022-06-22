using ECS;
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
                Id<Transform>(),
                Id<MeleeAttackReachComponent>(),
                Id<MeleeDamageComponent>(),
                Id<MeleeAttackComponent>()
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
            var attackReach = world.GetComponent<MeleeAttackReachComponent>(entity).distance;
            var distance = (targetTransform.position - transform.position).magnitude;
            if (distance > attackReach)
                continue;

            ref var attackComponent = ref world.GetComponent<MeleeAttackComponent>(entity);
            var nextAttackTime = attackComponent.previousAttackTime + attackComponent.attackCD;
            if (Time.time < nextAttackTime)
                continue;

            var damage = world.GetComponent<MeleeDamageComponent>(entity).damage;
            Debug.Log("Enemy attack!");
            world.GetComponent<HealthComponent>(playerEntity).health -= damage;
            attackComponent.previousAttackTime = Time.time;
        }
    }
}
