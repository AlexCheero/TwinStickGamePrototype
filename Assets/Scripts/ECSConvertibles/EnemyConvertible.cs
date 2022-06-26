using Components;
using ECS;
using Tags;
using UnityEngine;
using UnityEngine.AI;

public class EnemyConvertible : ECSConvertible
{
    [SerializeField]
    private float _speed;

    [SerializeField]
    private float _angularSpeed;
    
    [SerializeField]
    private float _acceleration;

    [SerializeField]
    private float _health;

    [SerializeField]
    private float _meleeAttackReach;

    [SerializeField]
    private float _meleeAttackCD;

    [SerializeField]
    private float _meleeDamage;

    protected override void AddComponents(EcsWorld world)
    {
        var id = Entity.GetId();
        world.AddTag<EnemyTag>(id);
        world.AddTag<MeleeWeaponHoldingTag>(id);
        world.AddComponent(id, transform);
        world.AddComponent(id, new HealthComponent { health = _health });
        world.AddComponent(id, new AttackComponent { previousAttackTime = -1, attackCD = _meleeAttackCD }) ;
        world.AddComponent(id, new DamageComponent { damage = _meleeDamage });

        AddTarget(world, id);

        var speed = world.AddComponent(id, new SpeedComponent { speed = _speed }).speed;
        var angularSpeed = world.AddComponent(id, new AngularSpeedComponent { speed = _angularSpeed }).speed;
        var attackReach = world.AddComponent(id, new ReachComponent { distance = _meleeAttackReach }).distance;
        var acceleration = world.AddComponent(id, new AccelerationComponent { acceleration = _acceleration }).acceleration;
        
        var navAgent = world.AddComponent(id, GetComponent<NavMeshAgent>());
        navAgent.speed = speed;
        navAgent.stoppingDistance = attackReach;
        navAgent.angularSpeed = angularSpeed;
        navAgent.acceleration = acceleration;
    }

    private void AddTarget(EcsWorld world, int entity)
    {
        var playerFilterIdIncludes =
            new BitMask(
                ComponentMeta<PlayerTag>.Id,
                ComponentMeta<Transform>.Id
            );
        var playerFilterId = world.RegisterFilter(playerFilterIdIncludes);
        foreach (var playerEntity in world.Enumerate(playerFilterId))
        {
            var targetTransform = world.GetComponent<Transform>(playerEntity);
            world.AddComponent(entity, new TargetTransformComponent { target = targetTransform });
            break;
        }
    }
}
