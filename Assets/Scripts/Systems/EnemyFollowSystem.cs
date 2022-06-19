using ECS;
using UnityEngine;

public class EnemyFollowSystem : EcsSystem
{
    private int _playerFilterId;
    private int _enemyFilterId;

    private Transform _target;

    public EnemyFollowSystem(EcsWorld world)
    {
        _playerFilterId = world.RegisterFilter(new BitMask(Id<PlayerTag>(), Id<Transform>()));
        _enemyFilterId = world.RegisterFilter(
            new BitMask(
                Id<EnemyTag>(),
                Id<Transform>(),
                Id<SpeedComponent>(),
                Id<MeleeAttackReachComponent>()
                ));
    }

    public override void Tick(EcsWorld world)
    {
        if (_target == null)
        {
            world.GetFilter(_playerFilterId).Iterate((entities, count) =>
            {
                if (count > 0)
                    _target = world.GetComponent<Transform>(entities[0]);
            });
        }

        if (_target == null)
            return;

        world.GetFilter(_enemyFilterId).Iterate((entities, count) =>
        {
            for (int i = 0; i < count; i++)
            {
                var transform = world.GetComponent<Transform>(entities[i]);
                var speed = world.GetComponent<SpeedComponent>(entities[i]).speed;
                var vectorToTarget = _target.position - transform.position;
                var distance = vectorToTarget.magnitude;
                var attackReach = world.GetComponent<MeleeAttackReachComponent>(entities[i]).distance;
                if (distance <= attackReach)
                    continue;

                var direction = vectorToTarget.normalized;
                transform.rotation = Quaternion.LookRotation(direction);
                transform.Translate(direction * speed * Time.deltaTime, Space.World);
            }
        });
    }
}
