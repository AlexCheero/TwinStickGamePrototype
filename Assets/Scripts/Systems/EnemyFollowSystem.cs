using ECS;
using UnityEngine;

public class EnemyFollowSystem : EcsSystem
{
    private int _playerFilterId;
    private int _enemyFilterId;

    //TODO: it is cached somwhere and somwhere not. make unified
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
            foreach (var entity in world.Enumerate(_playerFilterId))
            {
                _target = world.GetComponent<Transform>(entity);
                break;
            }
        }

        if (_target == null)
            return;

        foreach (var entity in world.Enumerate(_enemyFilterId))
        {
            var transform = world.GetComponent<Transform>(entity);
            var speed = world.GetComponent<SpeedComponent>(entity).speed;
            var vectorToTarget = _target.position - transform.position;
            var distance = vectorToTarget.magnitude;
            var attackReach = world.GetComponent<MeleeAttackReachComponent>(entity).distance;
            if (distance <= attackReach)
                continue;

            var direction = vectorToTarget.normalized;
            transform.rotation = Quaternion.LookRotation(direction);
            transform.Translate(direction * speed * Time.deltaTime, Space.World);
        }
    }
}
