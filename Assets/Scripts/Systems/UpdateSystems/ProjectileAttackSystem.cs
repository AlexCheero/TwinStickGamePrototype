using Components;
using ECS;
using Tags;
using UnityEngine;

[System(ESystemCategory.Update)]
public class ProjectileAttackSystem : EcsSystem
{
    private int _startAttackFilterId;
    private int _tagFallThroughFilterId;
    private int _spawnProjectileFilterId;

    public ProjectileAttackSystem(EcsWorld world)
    {
        _startAttackFilterId = world.RegisterFilter(new BitMask(Id<Attack>(),
                                                                Id<ProjectileWeapon>(),
                                                                Id<Ammo>()),
                                                    new BitMask(Id<GrenadeFly>()));

        _tagFallThroughFilterId = world.RegisterFilter(new BitMask(Id<CharacterTag>(), Id<CurrentWeapon>(), Id<GrenadeFly>()));

        _spawnProjectileFilterId = world.RegisterFilter(new BitMask(Id<GrenadeFly>(),
                                                                    Id<ProjectileWeapon>(),
                                                                    Id<Ammo>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_startAttackFilterId))
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

            ammo--;

#if DEBUG
            if (!world.Have<Owner>(id))
                throw new System.Exception("weapon should have owner");
#endif
            var ownerId = world.GetComponent<Owner>(id).entity.GetId();
            if (world.Have<Animator>(ownerId))
                world.GetComponent<Animator>(ownerId).SetTrigger("IsThrowing");

            world.Remove<Attack>(id);
        }

        foreach (var id in world.Enumerate(_tagFallThroughFilterId))
        {
            var weaponId = world.GetComponent<CurrentWeapon>(id).entity.GetId();
            world.Add(weaponId, world.GetComponent<GrenadeFly>(id));
            world.Remove<GrenadeFly>(id);
        }

        foreach (var id in world.Enumerate(_spawnProjectileFilterId))
        {
            var attack = world.GetComponent<GrenadeFly>(id);

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

            world.Remove<GrenadeFly>(id);
        }
    }
}