using ECS;
using Components;
using Tags;
using UnityEngine;

[ReactiveSystem(EReactionType.OnAdd, typeof(CollisionWith))]
public static class OnWeaponPickupCollision
{
    private static BitMask _includes = new BitMask(ComponentMeta<Pickup>.Id,
                                                   ComponentMeta<Weapon>.Id,
                                                   ComponentMeta<Transform>.Id);

    public static void Tick(EcsWorld world, int id)
    {
        if (world.CheckAgainstMasks(id, _includes))
        {
            Debug.Log("new weapon picked up");
            var collidedEntity = world.GetComponent<CollisionWith>(id).entity;
            var collidedId = collidedEntity.GetId();
            if (world.IsEntityValid(collidedEntity) &&
                world.Have<PlayerTag>(collidedId))
            {
                world.GetOrAddComponentRef<CurrentWeapon>(collidedId).entity = world.GetById(id);
                Object.Destroy(world.GetComponent<Transform>(id).gameObject);
                world.Remove<Transform>(id);
            }
        }
    }
}
