using Components;
using ECS;
using Tags;
using UnityEngine;

//choose system type here
[System(ESystemCategory.Init)]
public class InitViewOffsetSystem : EcsSystem
{
    private readonly int _filterId;

    public InitViewOffsetSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<Collider>(), Id<CharacterTag>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            var bounds = world.GetComponent<Collider>(id).bounds;
            var height = bounds.extents.y * 2;
            //3/4 upper part of collider
            world.GetOrAddComponent<ViewOffset>(id).offset = new Vector3(0, height * 3 / 4, 0);
        }
    }
}