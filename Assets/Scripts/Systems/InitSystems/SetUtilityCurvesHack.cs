using Components;
using ECS;
using Tags;
using UnityEngine;

//choose system type here
[System(ESystemCategory.Init)]
public class SetUtilityCurvesHack : EcsSystem
{
    private int _filterId;
    private UtilityCurves _curves;

    public SetUtilityCurvesHack(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<EnemyTag>()));
        _curves = Object.FindObjectOfType<UtilityCurvesHolder>().Curves;
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
            world.Add(id, new UtilityCurvesComponent { curves = _curves });
    }
}