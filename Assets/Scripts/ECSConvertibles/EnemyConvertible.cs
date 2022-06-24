using ECS;
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

    public override void ConvertToEntity(EcsWorld world)
    {
        var entity = world.Create();
        world.AddTag<EnemyTag>(entity);
        world.AddTag<MeleeWeaponHoldingTag>(entity);
        world.AddComponent(entity, transform);
        world.AddComponent(entity, new HealthComponent { health = _health });
        world.AddComponent(entity, new AttackComponent { previousAttackTime = -1, attackCD = _meleeAttackCD }) ;
        world.AddComponent(entity, new DamageComponent { damage = _meleeDamage });

        AddTarget(world, entity);

        var speed = world.AddComponent(entity, new SpeedComponent { speed = _speed }).speed;
        var angularSpeed = world.AddComponent(entity, new AngularSpeedComponent { speed = _angularSpeed }).speed;
        var attackReach = world.AddComponent(entity, new ReachComponent { distance = _meleeAttackReach }).distance;
        var acceleration = world.AddComponent(entity, new AccelerationComponent { acceleration = _acceleration }).acceleration;
        
        var navAgent = world.AddComponent(entity, GetComponent<NavMeshAgent>());
        navAgent.speed = speed;
        navAgent.stoppingDistance = attackReach;
        navAgent.angularSpeed = angularSpeed;
        navAgent.acceleration = acceleration;

        var view = GetComponent<EntityView>();
        if (view != null)
            view.Entity = world.GetById(entity);
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
