using ECS;
using UnityEngine;

public class ProjectileConvertible : ECSConvertible
{
    [SerializeField]
    private float _damage;
    [SerializeField]
    private float _speed;

    protected override void AddComponents(EcsWorld world)
    {
        var id = Entity.GetId();
        world.AddTag<Projectile>(id);
        world.AddComponent(id, new DamageComponent { damage = _damage });
        world.AddComponent(id, new SpeedComponent { speed = _speed });
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Projectile collided");
        var view = collision.gameObject.GetComponent<ECSConvertible>();
        if (view != null)
        {
            var collidedEntityId = view.Entity.GetId();
            if (_world.Have<HealthComponent>(collidedEntityId))
            {
                Debug.Log("Projectile damaged!");
                _world.GetComponent<HealthComponent>(collidedEntityId).health -=
                    _world.GetComponent<DamageComponent>(Entity.GetId()).damage;
            }
        }

        _world.Delete(Entity.GetId());
        Destroy(gameObject);
    }
}
