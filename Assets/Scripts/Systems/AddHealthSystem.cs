using Components;
using ECS;
using Tags;

[UpdateSystem]
public class AddHealthSystem : EcsSystem
{
    private int _filterId;

    public AddHealthSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<Pickup>(),
            Id<AddHealth>(),
            Id<DeleteOnCollision>(),
            Id<CollisionWith>()
            ));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            var collidedEntity = world.GetComponent<CollisionWith>(id).entity;
            var collidedId = collidedEntity.GetId();
            if (world.IsEntityValid(collidedEntity) && world.Have<HealthComponent>(collidedId))
            {
                world.GetComponent<HealthComponent>(collidedId).health +=
                    world.GetComponent<AddHealth>(id).healthAmount;

                world.AddTag<DeadTag>(id);
            }
        }
    }
}