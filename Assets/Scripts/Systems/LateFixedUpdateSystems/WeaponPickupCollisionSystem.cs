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
            var collidedEntity = world.GetComponent<CollisionWith>(id).entity;
            var collidedId = collidedEntity.GetId();
            if (world.IsEntityValid(collidedEntity) &&
                world.Have<PlayerTag>(collidedId))
            {
                var weaponEntity = world.GetById(id);
                world.GetOrAddComponentRef<CurrentWeapon>(collidedId).entity = weaponEntity;
                var attackReach = world.Have<ReachComponent>(id) ? world.GetComponent<ReachComponent>(id).distance : float.PositiveInfinity;
                world.GetOrAddComponentRef<AttackReachComponent>(collidedId).distance = attackReach;
                world.GetOrAddComponentRef<Owner>(id).entity = collidedEntity;

                var weaponTransform = world.GetComponent<Transform>(id);
                var weaponCollider = weaponTransform.gameObject.GetComponent<Collider>();
                if (weaponCollider != null)
                    weaponCollider.enabled = false;

                var playerTransform = world.GetComponent<Transform>(collidedId);
                var gunHolder = MiscUtils.FindGrandChildByName(playerTransform, "GunHolder");
                foreach (Transform gun in gunHolder)
                    gun.gameObject.SetActive(false);

                weaponTransform.SetParent(gunHolder);

                if (world.Have<GripTransform>(id))
                {
                    var gripTransform = world.GetComponent<GripTransform>(id);
                    weaponTransform.localPosition = gripTransform.position;
                    weaponTransform.localEulerAngles = gripTransform.rotation;
                }
                else
                {
                    weaponTransform.localPosition = Vector3.zero;
                    weaponTransform.localEulerAngles = Vector3.zero;
                }

                if (world.Have<Weaponry>(collidedId))
                {
                    ref var weaponry = ref world.GetComponentByRef<Weaponry>(collidedId);
                    if (world.Have<MeleeWeapon>(id))
                        weaponry.melee = weaponEntity;
                    else if (world.Have<RangedWeapon>(id))
                        weaponry.ranged = weaponEntity;
                    else if (world.Have<Projectile>(id))
                        weaponry.throwable = weaponEntity;
                }
            }
        }
    }
}