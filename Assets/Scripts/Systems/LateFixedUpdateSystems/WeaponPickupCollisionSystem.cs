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
                var GunHolder = FindGrandChildByName(playerTransform, "GunHolder");
                var GunGrip = FindGrandChildByName(weaponTransform, "Grip");

                weaponTransform.SetParent(GunHolder);
                weaponTransform.localPosition = -GunGrip.localPosition;
                weaponTransform.localRotation = Quaternion.identity;

                world.Remove<Transform>(id);
            }
        }
    }

    private Transform FindGrandChildByName(Transform transform, string name)
    {
        foreach (Transform child in transform)
        {
            if (child.name == name)
                return child;
            var childTransform = FindGrandChildByName(child, name);
            if (childTransform != null)
                return childTransform;
        }

        return null;
    }
}