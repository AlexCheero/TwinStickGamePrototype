using Components;
using ECS;
using Tags;
using UnityEngine;

[System(ESystemCategory.Update)]
public class PlayerAttackSystem : EcsSystem
{
    private readonly int _filterId;

    public PlayerAttackSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<PlayerTag>(), Id<CurrentWeapon>(), Id<PlayerSight>()));
    }

    public override void Tick(EcsWorld world)
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        foreach (var id in world.Enumerate(_filterId))
        {
            var weaponEntity = world.GetComponent<CurrentWeapon>(id).entity;
#if DEBUG
            if (!world.IsEntityValid(weaponEntity))
                throw new System.Exception("invalid weapon entity");
#endif
            var weaponId = weaponEntity.GetId();

#if DEBUG
            if (world.Have<AttackEvent>(weaponId))
                throw new System.Exception("please clean Attack component from weapon");
#endif

            var sight = world.GetComponent<PlayerSight>(id);
            world.Add(weaponId, new AttackEvent { position = sight.Start, direction = (sight.End - sight.Start).normalized });
        }
    }
}