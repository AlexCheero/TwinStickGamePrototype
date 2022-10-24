using Components;
using ECS;
using Tags;

[System(ESystemCategory.Init)]
public class InitDefaultWeaponSystem : EcsSystem
{
    private readonly int _defaultWeaponfilterId;
    private readonly int _weaponryfilterId;

    public InitDefaultWeaponSystem(EcsWorld world)
    {
        _defaultWeaponfilterId = world.RegisterFilter(new BitMask(Id<DefaultWeapon>()));
        _weaponryfilterId = world.RegisterFilter(new BitMask(Id<CurrentWeapon>(), Id<Weaponry>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_defaultWeaponfilterId))
        {
            var preset = world.GetComponent<DefaultWeapon>(id).preset;
            var weaponId = preset.InitAsEntity(world);
            world.GetOrAddComponentRef<CurrentWeapon>(id).entity = world.GetById(weaponId);
            var attackReach = world.Have<ReachComponent>(weaponId) ?
                world.GetComponent<ReachComponent>(weaponId).distance :
                float.MaxValue;
            world.GetOrAddComponentRef<AttackReachComponent>(id).distance = attackReach;
            world.GetOrAddComponentRef<Owner>(weaponId).entity = world.GetById(id);
        }

        foreach (var id in world.Enumerate(_weaponryfilterId))
        {
            var weaponId = world.GetComponent<CurrentWeapon>(id).entity.GetId();
            ref var weaponry = ref world.GetComponentByRef<Weaponry>(id);
            if (world.Have<MeleeWeapon>(weaponId))
            {
                weaponry.melee = world.GetById(weaponId);
                weaponry.ranged = weaponry.throwable = EntityExtension.NullEntity;
            }
            else if (world.Have<RangedWeapon>(weaponId))
            {
                weaponry.ranged = world.GetById(weaponId);
                weaponry.melee = weaponry.throwable = EntityExtension.NullEntity;
            }
            else if (world.Have<Projectile>(weaponId))
            {
                weaponry.throwable = world.GetById(weaponId);
                weaponry.melee = weaponry.throwable = EntityExtension.NullEntity;
            }
        }
    }
}