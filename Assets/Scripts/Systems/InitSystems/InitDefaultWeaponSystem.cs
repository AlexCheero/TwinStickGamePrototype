using Components;
using ECS;

[System(ESystemCategory.Init)]
public class InitDefaultWeaponSystem : EcsSystem
{
    private int _filterId;

    public InitDefaultWeaponSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<DefaultWeapon>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            var preset = world.GetComponent<DefaultWeapon>(id).preset;
            var weaponId = preset.InitAsEntity(world);
            world.GetOrAddComponentRef<CurrentWeapon>(id).entity = world.GetById(weaponId);
            var attackReach = world.Have<ReachComponent>(weaponId) ?
                world.GetComponent<ReachComponent>(weaponId).distance :
                float.MaxValue;
            world.GetOrAddComponentRef<AttackReachComponent>(id).distance = attackReach;
        }
    }
}