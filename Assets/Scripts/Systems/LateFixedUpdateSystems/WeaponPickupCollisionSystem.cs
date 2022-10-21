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
                var gunHolder = MiscUtils.FindGrandChildByName(playerTransform, "GunHolder");
                foreach (Transform gun in gunHolder)
                    GameObject.Destroy(gun.gameObject);
                var gunGrip = MiscUtils.FindGrandChildByName(weaponTransform, "Grip");

                weaponTransform.SetParent(gunHolder);

                if (gunGrip != null)
                {
                    weaponTransform.localPosition = -gunGrip.localPosition;
                    weaponTransform.localRotation = gunGrip.localRotation;
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