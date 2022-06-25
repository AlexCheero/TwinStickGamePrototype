using ECS;
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

        foreach (var entity in world.Enumerate(_filterId))
        {
            Debug.Log("Player projectile attack!");

            var transform = world.GetComponent<Transform>(entity);
            var projectileObj = world.GetComponent<ProjectileWeapon>(entity).projectile;

            var instantiationPosition = transform.position + transform.forward * 2.0f; //instantiation before the player
            //TODO: use pools
            var projectileInstance = Object.Instantiate(projectileObj, instantiationPosition, transform.rotation);
            var projectileEntity = projectileInstance.ConvertToEntity(world);
            var speed = world.GetComponent<SpeedComponent>(projectileEntity).speed;
            projectileInstance.GetComponent<Rigidbody>().AddForce(transform.forward * speed);//TODO: try different force types
        }
    }
}
