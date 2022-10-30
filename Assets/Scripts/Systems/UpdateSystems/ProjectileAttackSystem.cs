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
        _startAttackFilterId = world.RegisterFilter(new BitMask(Id<AttackEvent>(),
                                                                Id<ProjectileWeapon>(),
                                                                Id<Ammo>(),
                                                                Id<Owner>()),
                                                    new BitMask(Id<GrenadeFlyEvent>()));

        _tagFallThroughFilterId = world.RegisterFilter(new BitMask(Id<CharacterTag>(), Id<CurrentWeapon>(), Id<GrenadeFlyEvent>()));

        _spawnProjectileFilterId = world.RegisterFilter(new BitMask(Id<GrenadeFlyEvent>(),
                                                                    Id<ProjectileWeapon>(),
                                                                    Id<Ammo>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_startAttackFilterId))
        {
            world.Remove<AttackEvent>(id);

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

            var ownerId = world.GetComponent<Owner>(id).entity.GetId();
            if (world.Have<Animator>(ownerId))
                world.GetComponent<Animator>(ownerId).SetTrigger("IsThrowing");
        }

        foreach (var id in world.Enumerate(_tagFallThroughFilterId))
        {
            var weaponId = world.GetComponent<CurrentWeapon>(id).entity.GetId();
            world.Add(weaponId, world.GetComponent<GrenadeFlyEvent>(id));
            world.Remove<GrenadeFlyEvent>(id);
        }

        foreach (var id in world.Enumerate(_spawnProjectileFilterId))
        {
            var attack = world.GetComponent<GrenadeFlyEvent>(id);

            var instantiationPosition = attack.position + attack.direction; //instantiation before the player

            var projectileWeapon = world.GetComponent<ProjectileWeapon>(id);
            var projectileView = PoolManager.Get(projectileWeapon.poolName).Get<EntityView>(instantiationPosition);
            var projectileId = projectileView.InitAsEntity(world);
#if DEBUG
            if (!world.Have<Projectile>(projectileId))
                throw new System.Exception("projectileView have no Projectile tag");
#endif
            var speed = world.GetComponent<SpeedComponent>(projectileId).speed;
            projectileView.GetComponent<Rigidbody>().AddForce(attack.direction * speed);//TODO: try different force types

            world.Remove<GrenadeFlyEvent>(id);
        }
    }
}