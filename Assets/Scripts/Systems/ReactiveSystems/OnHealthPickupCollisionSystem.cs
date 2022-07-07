using ECS;
using Components;
using Tags;

[ReactiveSystem(EReactionType.OnAdd, typeof(CollisionWith))]
public static class OnHealthPickupCollisionSystem
{
    private static BitMask _includes = new BitMask(
        ComponentMeta<Pickup>.Id,
        ComponentMeta<HealthComponent>.Id,
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
                world.GetComponent<HealthComponent>(collidedId).health +=
                    world.GetComponent<HealthComponent>(id).health;

                world.AddTag<DeadTag>(id);
            }
        }
    }
}