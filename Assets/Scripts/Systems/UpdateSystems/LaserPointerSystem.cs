using Components;
using ECS;
using UnityEngine;

//choose system type here
[System(ESystemCategory.Update)]
public class LaserPointerSystem : EcsSystem
{
    private readonly int _filterId;

    public LaserPointerSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<PlayerSight>(), Id<CurrentWeapon>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            var currWeaponId = world.GetComponent<CurrentWeapon>(id).entity.GetId();
            if (!world.Have<LaserPointer>(currWeaponId))
                continue;
            var lpObject = world.GetComponent<LaserPointer>(currWeaponId).laser;
            var sight = world.GetComponent<PlayerSight>(id);
            lpObject.SetPosition(0, lpObject.transform.position);
            lpObject.SetPosition(1, sight.End);
            var color = sight.SightedView != null ? Color.red : Color.green;
            lpObject.startColor = color;
            lpObject.endColor = color;
        }
    }
}