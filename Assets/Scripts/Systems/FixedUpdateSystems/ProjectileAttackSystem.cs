using Components;
using ECS;
using Tags;
using UnityEngine;

[System(ESystemCategory.FixedUpdate)]
public class ProjectileAttackSystem : EcsSystem
{
    private int _filterId;

    public ProjectileAttackSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<Attack>(),
                                                     Id<ProjectileWeapon>(),
                                                     Id<Ammo>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
#if DEBUG
            if (world.GetComponent<Ammo>(id).amount <= 0)
                throw new System.Exception("OnProjectileShotSystem. ammo amount is <= 0. have ammo component: " + world.Have<Ammo>(id));
#endif

            var attack = world.GetComponent<Attack>(id);

            var instantiationPosition = attack.position + attack.direction * 2.0f; //instantiation before the player
            var projectileView = PoolManager.Get("ProjectilePool").Get<EntityView>(instantiationPosition, Quaternion.identity);
            var projectileId = projectileView.InitAsEntity(world);
#if DEBUG
            if (!world.Have<Projectile>(projectileId))
                throw new System.Exception("projectileView have no Projectile tag");
#endif
            var speed = world.GetComponent<SpeedComponent>(projectileId).speed;
            projectileView.GetComponent<Rigidbody>().AddForce(attack.direction * speed);//TODO: try different force types

            world.GetComponentByRef<Ammo>(id).amount--;
        }
    }
}