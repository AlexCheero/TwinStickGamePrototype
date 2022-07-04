using Components;
using ECS;
using Tags;
using UnityEngine;

[UpdateSystem]
public class ProjectileCollisionSystem : EcsSystem
{
    private int _filterId;

    public ProjectileCollisionSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(
            Id<Projectile>(),
            Id<DeleteOnCollision>(),
            Id<CollisionWith>(),
            Id<Transform>()
            ));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            Debug.Log("process projectile collision");
            var collidedEntity = world.GetComponent<CollisionWith>(id).entity;
            var collidedId = collidedEntity.GetId();
            //TODO: check what will be if remove component and then delete entity
            //if (!world.IsEntityValid(collidedEntity))
            //    world.RemoveComponent<CollisionWith>(id);
            var isValid = world.IsEntityValid(collidedEntity);
            if (world.IsEntityValid(collidedEntity) && world.Have<HealthComponent>(collidedId))
            {
                world.GetComponent<HealthComponent>(collidedId).health -=
                    world.GetComponent<DamageComponent>(id).damage;
            }

            Object.Destroy(world.GetComponent<Transform>(id).gameObject);
            world.Delete(id);
        }
    }
}