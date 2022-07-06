using ECS;
using Components;
using Tags;

[ReactiveSystem(EReactionType.OnAdd, typeof(CollisionWith))]
public static class OnProjectileCollisionSystem
{
    private static BitMask _includes = new BitMask(
        ComponentMeta<Projectile>.Id,
        ComponentMeta<DeleteOnCollision>.Id
        );

    public static void Tick(EcsWorld world, int id)
    {
        if (world.CheckAgainstMasks(id, _includes))
        {
            var collidedEntity = world.GetComponent<CollisionWith>(id).entity;
            var collidedId = collidedEntity.GetId();
            if (world.IsEntityValid(collidedEntity) && world.Have<HealthComponent>(collidedId))
            {
                world.GetComponent<HealthComponent>(collidedId).health -=
                    world.GetComponent<DamageComponent>(id).damage;
            }

            world.AddTag<DeadTag>(id);
        }
    }
}
