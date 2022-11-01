using System;
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

    private static bool IsRightAmmoType(EcsWorld world, Entity weaponEntity, int pickupId)
    {
#if DEBUG
        if (world.IsNull(weaponEntity.GetId()))
            return false;
        if (!world.IsEntityValid(weaponEntity))
            throw new Exception("weapon entity isn't null but invalid");
#else
        if (!world.IsEntityValid(weaponEntity))
            return false;
#endif
        var weaponId = weaponEntity.GetId();
        return world.Have<Ammo>(weaponId) && world.GetComponent<Ammo>(weaponId).type == world.GetComponent<Ammo>(pickupId).type;
    }

    private Entity GetWeaponIdForAmmoType(EcsWorld world, int weaponOwnerId, int pickupId)
    {
        var weaponry = world.GetComponent<Weaponry>(weaponOwnerId);
        if (IsRightAmmoType(world, weaponry.ranged, pickupId))
            return weaponry.ranged;
        if (IsRightAmmoType(world, weaponry.throwable, pickupId))
            return weaponry.throwable;
        if (IsRightAmmoType(world, weaponry.melee, pickupId))
            return weaponry.melee;
        return EntityExtension.NullEntity;
    }
    
    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            var collidedEntity = world.GetComponent<CollisionWith>(id).entity;
            var collidedId = collidedEntity.GetId();
            if (world.IsEntityValid(collidedEntity) && world.Have<Weaponry>(collidedId))
            {
                var weaponEntity = GetWeaponIdForAmmoType(world, collidedId, id);
                if (!world.IsEntityValid(weaponEntity))
                    continue;
                var weaponId = weaponEntity.GetId();
                world.GetComponentByRef<Ammo>(weaponId).amount += world.GetComponent<Ammo>(id).amount;
                world.Add<DeadTag>(id);
            }
        }
    }
}