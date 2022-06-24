using ECS;
using UnityEngine;

public class PlayerConvertible : ECSConvertible
{
    [SerializeField]
    private float _speed;

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
        world.AddTag<PlayerTag>(entity);
        world.AddTag<InstantRangedWeaponHoldingTag>(entity);
        world.AddComponent(entity, transform);
        world.AddComponent(entity, new SpeedComponent { speed = _speed });
        world.AddComponent(entity, new HealthComponent { health = _health });
        world.AddComponent(entity, new ReachComponent { distance = _meleeAttackReach });
        world.AddComponent(entity, new AttackComponent { previousAttackTime = -1, attackCD = _meleeAttackCD });
        world.AddComponent(entity, new DamageComponent { damage = _meleeDamage });
    }
}
