using Components;
using ECS;
using Tags;
using UnityEngine;

public class PlayerProjectileAttackSystem : EcsSystem
{
    private int _filterId;

    public PlayerProjectileAttackSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(
            new BitMask(
                Id<PlayerTag>(),
                Id<ProjectileWeaponHoldingTag>(),
                Id<ProjectileWeapon>(),
                Id<Transform>()
                ));
    }

    public override void Tick(EcsWorld world)
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        foreach (var id in world.Enumerate(_filterId))
        {
            Debug.Log("Player projectile attack!");

            var transform = world.GetComponent<Transform>(id);
            var projectileObj = world.GetComponent<ProjectileWeapon>(id).projectile;

            var instantiationPosition = transform.position + transform.forward * 2.0f; //instantiation before the player
            //TODO: use pools
            var projectileView = Object.Instantiate(projectileObj, instantiationPosition, transform.rotation);
            var projectileId = projectileView.InitAsEntity(world);
#if DEBUG
            if (!world.Have<Projectile>(projectileId))
                throw new System.Exception("projectileView have no Projectile tag");
#endif
            var speed = world.GetComponent<SpeedComponent>(projectileId).speed;
            projectileView.GetComponent<Rigidbody>().AddForce(transform.forward * speed);//TODO: try different force types
        }
    }
}