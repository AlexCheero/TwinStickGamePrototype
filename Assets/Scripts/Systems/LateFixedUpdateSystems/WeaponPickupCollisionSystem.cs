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
                var attackReach = world.Have<ReachComponent>(id) ? world.GetComponent<ReachComponent>(id).distance : float.PositiveInfinity;
                world.GetOrAddComponentRef<AttackReachComponent>(collidedId).distance = attackReach;
                world.GetOrAddComponentRef<Owner>(id).entity = collidedEntity;

                var weaponTransform = world.GetComponent<Transform>(id);
                var weaponCollider = weaponTransform.gameObject.GetComponent<Collider>();
                if (weaponCollider != null)
                    weaponCollider.enabled = false;

                var playerTransform = world.GetComponent<Transform>(collidedId);
                var GunHolder = MiscUtils.FindGrandChildByName(playerTransform, "GunHolder");
                var GunGrip = MiscUtils.FindGrandChildByName(weaponTransform, "Grip");

                weaponTransform.SetParent(GunHolder);

                if (GunGrip != null)
                {
                    weaponTransform.localPosition = -GunGrip.localPosition;
                    weaponTransform.localRotation = GunGrip.localRotation;
                }
                else
                {
                    weaponTransform.localPosition = Vector3.zero;
                    weaponTransform.localRotation = Quaternion.identity;
                }

                world.Remove<Transform>(id);
            }
        }
    }
}