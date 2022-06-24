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
    [SerializeField]
    private ProjectileConvertible _projectile;

    protected override void AddComponents(EcsWorld world)
    {
        var id = Entity.GetId();
        world.AddTag<PlayerTag>(id);
        //world.AddTag<InstantRangedWeaponHoldingTag>(id);
        //world.AddTag<MeleeWeaponHoldingTag>(id);
        world.AddTag<ProjectileWeaponHoldingTag>(id);
        world.AddComponent(id, transform);
        world.AddComponent(id, new SpeedComponent { speed = _speed });
        world.AddComponent(id, new HealthComponent { health = _health });
        world.AddComponent(id, new ReachComponent { distance = _meleeAttackReach });
        world.AddComponent(id, new AttackComponent { previousAttackTime = -1, attackCD = _meleeAttackCD });
        world.AddComponent(id, new DamageComponent { damage = _meleeDamage });
        world.AddComponent(id, new ProjectileWeapon { projectile = _projectile });
    }
}
