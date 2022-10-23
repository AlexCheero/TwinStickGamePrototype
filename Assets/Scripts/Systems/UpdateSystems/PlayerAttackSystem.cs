using Components;
using ECS;
using Tags;
using UnityEngine;

[System(ESystemCategory.Update)]
public class PlayerAttackSystem : EcsSystem
{
    private int _filterId;

    public PlayerAttackSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<PlayerTag>(), Id<CurrentWeapon>(), Id<Transform>()));
    }

    public override void Tick(EcsWorld world)
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        foreach (var id in world.Enumerate(_filterId))
        {
            var weaponEntity = world.GetComponentByRef<CurrentWeapon>(id).entity;
#if DEBUG
            if (!world.IsEntityValid(weaponEntity))
                throw new System.Exception("invalid weapon entity");
#endif
            var weaponId = weaponEntity.GetId();

#if DEBUG
            if (world.Have<AttackEvent>(weaponId))
                throw new System.Exception("please clean Attack component from weapon");
#endif

            var transform = world.GetComponent<Transform>(id);
            var pos = transform.position;
            pos.y += 1.5f;
            world.Add(weaponId, new AttackEvent { position = pos, direction = transform.forward });
        }
    }
}