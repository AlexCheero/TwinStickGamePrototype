using Components;
using ECS;
using Tags;

[System(ESystemCategory.LateFixedUpdate)]
public class ProjectileCollisionSystem : EcsSystem
{
    private int _filterId;

    public ProjectileCollisionSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<Projectile>(), Id<DeleteOnCollision>(), Id<CollisionWith>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            var collidedEntity = world.GetComponent<CollisionWith>(id).entity;
            var collidedId = collidedEntity.GetId();
            if (world.IsEntityValid(collidedEntity) &&
                world.Have<HealthComponent>(collidedId) &&
                !world.Have<Pickup>(collidedId))
            {
                world.GetComponentByRef<HealthComponent>(collidedId).health -=
                    world.GetComponent<DamageComponent>(id).damage;
            }

            world.Add<DeadTag>(id);
        }
    }
}