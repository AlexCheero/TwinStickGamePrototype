using ECS;
using UnityEngine;

public class EnemyConvertible : ECSConvertible
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
        world.AddTag<EnemyTag>(entity);
        world.AddComponent(entity, transform);
        world.AddComponent(entity, new SpeedComponent { speed = _speed });
        world.AddComponent(entity, new HealthComponent { health = _health });
        world.AddComponent(entity, new MeleeAttackReachComponent { distance = _meleeAttackReach });
        world.AddComponent(entity, new MeleeAttackComponent { previousAttackTime = -1, attackCD = _meleeAttackCD }) ;
        world.AddComponent(entity, new MeleeDamageComponent { damage = _meleeDamage });

        var view = GetComponent<EntityView>();
        if (view != null)
            view.Entity = world.GetById(entity);
    }
}
