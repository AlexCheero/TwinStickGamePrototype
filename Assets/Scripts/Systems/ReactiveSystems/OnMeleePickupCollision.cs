using ECS;
using Components;
using Tags;

[ReactiveSystem(EReactionType.OnAdd, typeof(CollisionWith))]
public static class OnMeleePickupCollision
{
    private static BitMask _includes = new BitMask(
        ComponentMeta<Pickup>.Id,
        ComponentMeta<MeleeWeaponHoldingTag>.Id,//TODO: add ANY filter, to use any of Melee/Range/Projectile weapon piskups in same system
        ComponentMeta<DeleteOnCollision>.Id
        );

    public static void Tick(EcsWorld world, int id)
    {
        if (world.CheckAgainstMasks(id, _includes))
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
    }
}
