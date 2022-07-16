using Components;
using ECS;
using Tags;
using UnityEngine;

[System(ESystemCategory.LateFixedUpdate)]
public class WeaponPickupCollisionSystem : EcsSystem
{
    private int _filterId;

    public WeaponPickupCollisionSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<CollisionWith>(),
                                                     Id<Pickup>(),
                                                     Id<Weapon>(),
                                                     Id<Transform>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
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
            else
            {
                //TODO: move to cleanup system in pickup systems group
                world.Remove<CollisionWith>(id);
            }
        }
    }
}