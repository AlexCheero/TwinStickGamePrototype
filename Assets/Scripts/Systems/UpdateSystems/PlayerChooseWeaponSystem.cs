using Components;
using ECS;
using Tags;
using UnityEngine;

//choose system type here
[System(ESystemCategory.Update)]
public class PlayerChooseWeaponSystem : EcsSystem
{
    private readonly int _filterId;

    public PlayerChooseWeaponSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<PlayerTag>(),
                                                     Id<CurrentWeapon>(),
                                                     Id<Weaponry>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            ref var currentWeapon = ref world.GetComponent<CurrentWeapon>(id);
            var weaponry = world.GetComponent<Weaponry>(id);
            if (Input.GetKeyDown(KeyCode.Alpha1))
                WeaponHelper.ChooseMelee(world, ref currentWeapon, weaponry);
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                WeaponHelper.ChooseRanged(world, ref currentWeapon, weaponry);
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                WeaponHelper.ChooseProjectile(world, ref currentWeapon, weaponry);
        }
    }
}