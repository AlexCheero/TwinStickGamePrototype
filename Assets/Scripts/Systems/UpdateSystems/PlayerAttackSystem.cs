using Components;
using ECS;
using Tags;
using UnityEngine;

[UpdateSystem]
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
            var transform = world.GetComponent<Transform>(id);
            var weaponId = weaponEntity.GetId();

#if DEBUG
            if (world.Have<Attack>(weaponId))
                throw new System.Exception("please clean Attack component from weapon");
#endif

            var shot = new Attack { position = transform.position, direction = transform.forward };
            world.Add(weaponId, shot);
        }
    }
}