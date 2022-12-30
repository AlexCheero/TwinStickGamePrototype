using System;
using Components;
using ECS;
using Tags;
using UnityEngine;

public static class WeaponHelper
{
    public static void TakeWeapon(EcsWorld world, int playerId, int weaponId)
    {
        var playerEntity = world.GetById(playerId);
#if DEBUG
        if (!world.IsEntityValid(playerEntity) || !world.Have<PlayerTag>(playerId))
            throw new Exception("wrong player entity");
#endif

        var weaponEntity = world.GetById(weaponId);
        world.GetOrAddComponent<CurrentWeapon>(playerId).entity = weaponEntity;
        var attackReach = world.Have<ReachComponent>(weaponId)
            ? world.GetComponent<ReachComponent>(weaponId).distance
            : float.PositiveInfinity;
        world.GetOrAddComponent<AttackReachComponent>(playerId).distance = attackReach;
        world.GetOrAddComponent<Owner>(weaponId).entity = playerEntity;

        var weaponTransform = world.GetComponent<Transform>(weaponId);
        var weaponCollider = weaponTransform.gameObject.GetComponent<Collider>();
        if (weaponCollider != null)
            weaponCollider.enabled = false;

        var playerTransform = world.GetComponent<Transform>(playerId);
        var gunHolder = MiscUtils.FindGrandChildByName(playerTransform, "GunHolder");
        foreach (Transform gun in gunHolder)
            gun.gameObject.SetActive(false);

        weaponTransform.SetParent(gunHolder);

        if (world.Have<GripTransform>(weaponId))
        {
            var gripTransform = world.GetComponent<GripTransform>(weaponId);
            weaponTransform.localPosition = gripTransform.position;
            weaponTransform.localEulerAngles = gripTransform.rotation;
        }
        else
        {
            weaponTransform.localPosition = Vector3.zero;
            weaponTransform.localEulerAngles = Vector3.zero;
        }

        if (world.Have<Weaponry>(playerId))
        {
            ref var weaponry = ref world.GetComponent<Weaponry>(playerId);
            if (world.Have<MeleeWeapon>(weaponId))
                weaponry.melee = weaponEntity;
            else if (world.Have<RangedWeapon>(weaponId))
                weaponry.ranged = weaponEntity;
            else if (world.Have<ProjectileWeapon>(weaponId))
                weaponry.throwable = weaponEntity;
        }

        // if (world.Have<Prototype>(weaponId))
        // {
        //     var key = world.GetComponent<Weapon>(weaponId).id;
        //     var stashedWeapons = EntityStashHolder.Instance.Weapons;
        //     if (!stashedWeapons.ContainsKey(key))
        //         stashedWeapons.Add(key, world.GetComponent<Prototype>(weaponId).prefab);
        // }
    }
    
    public static void ChooseMelee(EcsWorld world, ref CurrentWeapon currentWeapon, Weaponry weaponry)
    {
        var currentWeaponId = currentWeapon.entity.GetId();
        if (world.Have<MeleeWeapon>(currentWeaponId) || !world.IsEntityValid(weaponry.melee))
            return;
        
        if (world.Have<Transform>(currentWeaponId))
            world.GetComponent<Transform>(currentWeaponId).gameObject.SetActive(false);
        currentWeapon.entity = weaponry.melee;
        currentWeaponId = weaponry.melee.GetId();
        if (world.Have<Transform>(currentWeaponId))
            world.GetComponent<Transform>(currentWeaponId).gameObject.SetActive(true);
    }
    
    public static void ChooseRanged(EcsWorld world, ref CurrentWeapon currentWeapon, Weaponry weaponry)
    {
        var currentWeaponId = currentWeapon.entity.GetId();
        if (world.Have<RangedWeapon>(currentWeaponId) || !world.IsEntityValid(weaponry.ranged))
            return;
        
        if (world.Have<Transform>(currentWeaponId))
            world.GetComponent<Transform>(currentWeaponId).gameObject.SetActive(false);
        currentWeapon.entity = weaponry.ranged;
        currentWeaponId = weaponry.ranged.GetId();
        if (world.Have<Transform>(currentWeaponId))
            world.GetComponent<Transform>(currentWeaponId).gameObject.SetActive(true);
    }
    
    public static void ChooseProjectile(EcsWorld world, ref CurrentWeapon currentWeapon, Weaponry weaponry)
    {
        var currentWeaponId = currentWeapon.entity.GetId();
        if (world.Have<ProjectileWeapon>(currentWeaponId) || !world.IsEntityValid(weaponry.throwable))
            return;
        
        if (world.Have<Transform>(currentWeaponId))
            world.GetComponent<Transform>(currentWeaponId).gameObject.SetActive(false);
        currentWeapon.entity = weaponry.throwable;
        currentWeaponId = weaponry.throwable.GetId();
        if (world.Have<Transform>(currentWeaponId))
            world.GetComponent<Transform>(currentWeaponId).gameObject.SetActive(true);
    }
    
    public static EWeaponType GetWeaponType(EcsWorld world, int weaponId)
    {
        if (world.Have<RangedWeapon>(weaponId))
            return EWeaponType.Ranged;
        if (world.Have<ProjectileWeapon>(weaponId))
            return EWeaponType.Projectile;
        return EWeaponType.Melee;
    }
}