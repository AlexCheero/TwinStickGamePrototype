using Components;
using ECS;
using Tags;

//choose system type here
[System(ESystemCategory.Update)]
public class ChangeWeaponIconUISystem : EcsSystem
{
    private int _filterId;

    public ChangeWeaponIconUISystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<CurrentWeapon>(), Id<WeaponUI>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            var currentWeaponId = world.GetComponent<CurrentWeapon>(id).entity.GetId();
            var ui = world.GetComponent<WeaponUI>(id).holder;
            if (world.Have<DefaultMeleeWeapon>(currentWeaponId))
                ui.SetIcon(0);
            else if (world.Have<MeleeWeapon>(currentWeaponId))
                ui.SetIcon(1);
            else if (world.Have<RangedWeapon>(currentWeaponId))
                ui.SetIcon(2);
            else if (world.Have<ProjectileWeapon>(currentWeaponId))
                ui.SetIcon(3);
        }
    }
}