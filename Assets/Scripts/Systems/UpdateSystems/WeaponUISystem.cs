using Components;
using ECS;

//choose system type here
[System(ESystemCategory.Update)]
public class WeaponUISystem : EcsSystem
{
    private readonly int _filterId;

    public WeaponUISystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<CurrentWeapon>(), Id<WeaponUI>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            var weaponUI = world.GetComponent<WeaponUI>(id);
            var currentWeaponId = world.GetComponent<CurrentWeapon>(id).entity.GetId();
            if (world.Have<WeaponIcon>(currentWeaponId))
            {
                weaponUI.image.sprite = world.GetComponent<WeaponIcon>(currentWeaponId).icon;
            }
            if (world.Have<Ammo>(currentWeaponId))
            {
                weaponUI.text.gameObject.SetActive(true);
                weaponUI.text.text = world.GetComponent<Ammo>(currentWeaponId).amount.ToString();
            }
            else
            {
                weaponUI.text.gameObject.SetActive(false);
            }
        }
    }
}