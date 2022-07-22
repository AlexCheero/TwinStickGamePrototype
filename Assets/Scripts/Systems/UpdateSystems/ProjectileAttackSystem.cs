using Components;
using ECS;
using Tags;
using UnityEngine;

[System(ESystemCategory.Update)]
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
            ref var ammo = ref world.GetComponentByRef<Ammo>(id).amount;
            if (ammo == 0)
                continue;
#if DEBUG
            if (ammo < 0)
                throw new System.Exception("negative ammo");
            if (world.GetComponent<Ammo>(id).amount <= 0)
                throw new System.Exception("OnProjectileShotSystem. ammo amount is <= 0. have ammo component: " + world.Have<Ammo>(id));
#endif

            var attack = world.GetComponent<Attack>(id);

            var instantiationPosition = attack.position + attack.direction * 2.0f; //instantiation before the player

            var projectileWeapon = world.GetComponent<ProjectileWeapon>(id);
            var projectileView = PoolManager.Get(projectileWeapon.poolName).Get<EntityView>(instantiationPosition, Quaternion.identity);
            var projectileId = projectileView.InitAsEntity(world);
            world.Remove<Prototype>(projectileId);
#if DEBUG
            if (!world.Have<Projectile>(projectileId))
                throw new System.Exception("projectileView have no Projectile tag");
#endif
            var speed = world.GetComponent<SpeedComponent>(projectileId).speed;
            projectileView.GetComponent<Rigidbody>().AddForce(attack.direction * speed);//TODO: try different force types

            ammo--;
        }
    }
}