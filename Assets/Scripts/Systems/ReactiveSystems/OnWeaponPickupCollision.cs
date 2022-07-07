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
        if (world.CheckAgainstMasks(id, _meleeIncludes))
        {
            var collidedEntity = world.GetComponent<CollisionWith>(id).entity;
            var collidedId = collidedEntity.GetId();
            if (world.IsEntityValid(collidedEntity) && world.Have<PlayerTag>(collidedId))
            {
                if (!world.Have<MeleeWeaponHoldingTag>(collidedId))
                    world.AddTag<MeleeWeaponHoldingTag>(collidedId);
                world.AddTag<DeadTag>(id);
            }
        }

        if (world.CheckAgainstMasks(id, _instantIncludes))
        {
            var collidedEntity = world.GetComponent<CollisionWith>(id).entity;
            var collidedId = collidedEntity.GetId();
            if (world.IsEntityValid(collidedEntity) && world.Have<PlayerTag>(collidedId))
            {
                if (!world.Have<InstantRangedWeaponHoldingTag>(collidedId))
                    world.AddTag<InstantRangedWeaponHoldingTag>(collidedId);
                world.AddTag<DeadTag>(id);
            }
        }

        if (world.CheckAgainstMasks(id, _projectileIncludes))
        {
            var collidedEntity = world.GetComponent<CollisionWith>(id).entity;
            var collidedId = collidedEntity.GetId();
            if (world.IsEntityValid(collidedEntity) && world.Have<PlayerTag>(collidedId))
            {
                if (!world.Have<ProjectileWeaponHoldingTag>(collidedId))
                    world.AddTag<ProjectileWeaponHoldingTag>(collidedId);
                world.AddTag<DeadTag>(id);
            }
        }
    }
}
