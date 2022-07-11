using Components;
using ECS;
using Tags;
using UnityEngine;

public class PlayerProjectileAttackSystem : EcsSystem
{
    private int _filterId;
    private ObjectPool _projectilePool;

    public PlayerProjectileAttackSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(
            new BitMask(
                Id<PlayerTag>(),
                Id<ProjectileWeaponHoldingTag>(),
                Id<ProjectileWeapon>(),
                Id<AttackComponent>(),
                Id<Ammo>(),
                Id<Transform>()
                ));

        _projectilePool = GameObject.Find("ProjectilePool").GetComponent<ObjectPool>();
    }

    public override void Tick(EcsWorld world)
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        foreach (var id in world.Enumerate(_filterId))
        {
            ref var attackComponent = ref world.GetComponentByRef<AttackComponent>(id);
            var nextAttackTime = attackComponent.previousAttackTime + attackComponent.attackCD;
            if (Time.time < nextAttackTime)
                continue;
            attackComponent.previousAttackTime = Time.time;

#if DEBUG
            if (world.GetComponent<Ammo>(id).amount <= 0)
                throw new System.Exception("ammo amount is <= 0. have ammo component: " + world.Have<Ammo>(id));
#endif

            Debug.Log("Player projectile attack!");

            var transform = world.GetComponent<Transform>(id);
            var projectileObj = world.GetComponent<ProjectileWeapon>(id).projectile;

            var instantiationPosition = transform.position + transform.forward * 2.0f; //instantiation before the player
            var projectileView = _projectilePool.Get<EntityView>(instantiationPosition, transform.rotation);
            var projectileId = projectileView.InitAsEntity(world);
#if DEBUG
            if (!world.Have<Projectile>(projectileId))
                throw new System.Exception("projectileView have no Projectile tag");
#endif
            var speed = world.GetComponent<SpeedComponent>(projectileId).speed;
            projectileView.GetComponent<Rigidbody>().AddForce(transform.forward * speed);//TODO: try different force types

            var newAmmo = world.GetComponent<Ammo>(id);
            newAmmo.amount--;
            world.SetComponent(id, newAmmo);
        }
    }
}
