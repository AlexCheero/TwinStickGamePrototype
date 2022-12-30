using Components;
using ECS;
using Tags;
using UnityEngine;

//choose system type here
[System(ESystemCategory.Update)]
public class LaserPointerSystem : EcsSystem
{
    private readonly int _filterId;

    public LaserPointerSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<PlayerSight>()));
    }

    private LineRenderer lpObject;
    public override void Tick(EcsWorld world)
    {
        lpObject ??= GameObject.FindObjectOfType<LineRenderer>();
        foreach (var id in world.Enumerate(_filterId))
        {
            var sight = world.GetComponent<PlayerSight>(id);
            lpObject.SetPosition(0, sight.Start);
            lpObject.SetPosition(1, sight.End);
            var color = sight.SightedView != null ? Color.green : Color.red;
            lpObject.startColor = color;
            lpObject.endColor = color;
        }
    }
}