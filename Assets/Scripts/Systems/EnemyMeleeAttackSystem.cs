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
        int playerEntity = -1; ;
        world.GetFilter(_playerFilterId).Iterate((entities, count) => playerEntity = entities[0]);

        var targetTransform = world.GetComponent<Transform>(playerEntity);

        world.GetFilter(_enemyFilterId).Iterate((entities, count) =>
        {
            for (int i = 0; i < count; i++)
            {
                var transform = world.GetComponent<Transform>(entities[i]);
                var attackReach = world.GetComponent<MeleeAttackReachComponent>(entities[i]).distance;
                var distance = (targetTransform.position - transform.position).magnitude;
                if (distance > attackReach)
                    continue;

                ref var attackComponent = ref world.GetComponent<MeleeAttackComponent>(entities[i]);
                var nextAttackTime = attackComponent.previousAttackTime + attackComponent.attackCD;
                if (Time.time < nextAttackTime)
                    continue;

                var damage = world.GetComponent<MeleeDamageComponent>(entities[i]).damage;
                //ref var targetHealth = ref world.GetComponent<HealthComponent>(playerEntity).health;
                //targetHealth -= damage;
                Debug.Log("Attack!");
                world.GetComponent<HealthComponent>(playerEntity).health -= damage;
                attackComponent.previousAttackTime = Time.time;
            }
        });
    }
}
