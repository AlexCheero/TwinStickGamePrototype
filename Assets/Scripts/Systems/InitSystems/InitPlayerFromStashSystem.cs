using System;
using Components;
using ECS;
using Tags;
using UnityEngine;

//choose system type here
[System(ESystemCategory.Init)]
public class InitPlayerFromStashSystem : EcsSystem
{
    private readonly int _filterId;

    public InitPlayerFromStashSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<PlayerTag>(), Id<Transform>(), Id<CurrentWeapon>(),
            Id<Weaponry>()));
    }

    public override void Tick(EcsWorld world)
    {
        if (!EntityStashHolder.IsCreated)
            return;
        
        foreach (var id in world.Enumerate(_filterId))
        {
            world.GetComponent<HealthComponent>(id).health = EntityStashHolder.Instance.Health;
            var stashHolder = EntityStashHolder.Instance;

            InitAndTakeWeapon(world, stashHolder.Melee, id);
            InitAndTakeWeapon(world, stashHolder.Ranged, id);
            InitAndTakeWeapon(world, stashHolder.Projectile, id);

            var weaponry = world.GetComponent<Weaponry>(id);
            ref var currentWeapon = ref world.GetComponent<CurrentWeapon>(id);
            switch (stashHolder.CurrentWeaponType)
            {
                case EWeaponType.Melee:
                    WeaponHelper.ChooseMelee(world, ref currentWeapon, weaponry);
                    break;
                case EWeaponType.Ranged:
                    WeaponHelper.ChooseRanged(world, ref currentWeapon, weaponry);
                    break;
                case EWeaponType.Projectile:
                    WeaponHelper.ChooseProjectile(world, ref currentWeapon, weaponry);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void InitAndTakeWeapon(EcsWorld world, WeaponStashData weaponStash, int playerId)
    {
        if (weaponStash.Prefab == null)
            return;
        
        var weapon = GameObject.Instantiate(weaponStash.Prefab);
        weapon.InitAsEntity(world);
        if (world.Have<Ammo>(weapon.Id))
            world.GetComponent<Ammo>(weapon.Id).amount = weaponStash.Ammo;
        WeaponHelper.TakeWeapon(world, playerId, weapon.Id);
    }
}