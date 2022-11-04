using Components;
using ECS;

//choose system type here
[System(ESystemCategory.Update)]
public class ChangeWeaponIconUISystem : EcsSystem
{
    private readonly int _filterId;

    public ChangeWeaponIconUISystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<CurrentWeapon>(), Id<WeaponUI>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            var currentWeaponId = world.GetComponent<CurrentWeapon>(id).entity.GetId();
            if (world.Have<WeaponIcon>(currentWeaponId))
            {
                world.GetComponent<WeaponUI>(id).image.sprite = world.GetComponent<WeaponIcon>(currentWeaponId).icon;
            }
        }
    }
}