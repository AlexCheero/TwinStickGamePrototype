using ECS;
using Components;
using Tags;

[ReactiveSystem(EReactionType.OnAdd, typeof(CollisionWith))]
public static class OnWeaponPickupCollision
{
    private static BitMask _meleeIncludes = new BitMask(
        ComponentMeta<Pickup>.Id,
        //TODO: add ANY filter, to use any of Melee/Range/Projectile weapon piskups in same system
        ComponentMeta<MeleeWeaponHoldingTag>.Id,
        ComponentMeta<DeleteOnCollision>.Id
        );

    private static BitMask _instantIncludes = new BitMask(
        ComponentMeta<Pickup>.Id,
        ComponentMeta<InstantRangedWeaponHoldingTag>.Id,
        ComponentMeta<DeleteOnCollision>.Id
        );

    private static BitMask _projectileIncludes = new BitMask(
        ComponentMeta<Pickup>.Id,
        ComponentMeta<ProjectileWeaponHoldingTag>.Id,
        ComponentMeta<DeleteOnCollision>.Id
        );

    public static void Tick(EcsWorld world, int id)
    {
        Tick<MeleeWeaponHoldingTag>(world, id, _meleeIncludes);
        Tick<InstantRangedWeaponHoldingTag>(world, id, _instantIncludes);
        Tick<ProjectileWeaponHoldingTag>(world, id, _projectileIncludes);
    }

    private static void Tick<T>(EcsWorld world, int id, BitMask includes)
    {
        if (world.CheckAgainstMasks(id, includes))
        {
            var collidedEntity = world.GetComponent<CollisionWith>(id).entity;
            var collidedId = collidedEntity.GetId();
            if (world.IsEntityValid(collidedEntity) && world.Have<PlayerTag>(collidedId))
            {
                if (!world.Have<T>(collidedId))
                    world.Add<T>(collidedId);
                if (world.Have<Ammo>(id))
                {
                    var amount = world.GetComponent<Ammo>(id).amount;
                    if (world.Have<Ammo>(collidedId))
                        world.GetComponent<Ammo>(collidedId).amount = amount;
                    else
                        world.AddComponent(collidedId, new Ammo { amount = amount });
                }
                world.Add<DeadTag>(id);
            }
        }
    }
}
