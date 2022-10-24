using Components;
using ECS;
using Tags;

[System(ESystemCategory.LateFixedUpdate)]
public class AmmoPickupCollisionSystem : EcsSystem
{
    private int _filterId;

    public AmmoPickupCollisionSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<CollisionWith>(),
            Id<Pickup>(),
            Id<Ammo>(),
            Id<DeleteOnCollision>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            var collidedEntity = world.GetComponent<CollisionWith>(id).entity;
            var collidedId = collidedEntity.GetId();
            if (world.IsEntityValid(collidedEntity) && world.Have<CurrentWeapon>(collidedId))
            {
                var weaponId = world.GetComponent<CurrentWeapon>(collidedId).entity.GetId();
                if (world.Have<Ammo>(weaponId))
                    world.GetComponentByRef<Ammo>(weaponId).amount += world.GetComponent<Ammo>(id).amount;

                world.Add<DeadTag>(id);
            }
        }
    }
}