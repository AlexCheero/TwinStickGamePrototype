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
            var currentWeaponId = currentWeapon.entity.GetId();
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                if (!world.Have<MeleeWeapon>(currentWeaponId) && world.IsEntityValid(weaponry.melee))
                {
                    if (world.Have<Transform>(currentWeaponId))
                        world.GetComponent<Transform>(currentWeaponId).gameObject.SetActive(false);
                    currentWeapon.entity = weaponry.melee;
                    currentWeaponId = weaponry.melee.GetId();
                    if (world.Have<Transform>(currentWeaponId))
                        world.GetComponent<Transform>(currentWeaponId).gameObject.SetActive(true);
                }
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                if (!world.Have<RangedWeapon>(currentWeaponId) && world.IsEntityValid(weaponry.ranged))
                {
                    if (world.Have<Transform>(currentWeaponId))
                        world.GetComponent<Transform>(currentWeaponId).gameObject.SetActive(false);
                    currentWeapon.entity = weaponry.ranged;
                    currentWeaponId = weaponry.ranged.GetId();
                    if (world.Have<Transform>(currentWeaponId))
                        world.GetComponent<Transform>(currentWeaponId).gameObject.SetActive(true);
                }
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                if (!world.Have<ProjectileWeapon>(currentWeaponId) && world.IsEntityValid(weaponry.throwable))
                {
                    if (world.Have<Transform>(currentWeaponId))
                        world.GetComponent<Transform>(currentWeaponId).gameObject.SetActive(false);
                    currentWeapon.entity = weaponry.throwable;
                    currentWeaponId = weaponry.throwable.GetId();
                    if (world.Have<Transform>(currentWeaponId))
                        world.GetComponent<Transform>(currentWeaponId).gameObject.SetActive(true);
                }
            }
        }
    }
}