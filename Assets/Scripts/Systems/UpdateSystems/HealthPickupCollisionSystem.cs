using Components;
using ECS;
using Tags;

[UpdateSystem]
public class HealthPickupCollisionSystem : EcsSystem
{
    private int _filterId;

    public HealthPickupCollisionSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<CollisionWith>(),
                                                     Id<Pickup>(),
                                                     Id<HealthComponent>(),
                                                     Id<DeleteOnCollision>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            var collidedEntity = world.GetComponent<CollisionWith>(id).entity;
            var collidedId = collidedEntity.GetId();
            if (world.IsEntityValid(collidedEntity) && world.Have<HealthComponent>(collidedId))
            {
                world.GetComponentByRef<HealthComponent>(collidedId).health +=
                    world.GetComponent<HealthComponent>(id).health;

                world.Add<DeadTag>(id);
            }
            else
            {
                world.Remove<CollisionWith>(id);
            }
        }
    }
}