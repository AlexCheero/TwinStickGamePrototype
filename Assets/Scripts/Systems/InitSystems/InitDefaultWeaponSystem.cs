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
            var meleeId = preset.InitAsEntity(world);
            world.GetOrAddComponentRef<CurrentWeapon>(id).entity = world.GetById(meleeId);
        }
    }
}