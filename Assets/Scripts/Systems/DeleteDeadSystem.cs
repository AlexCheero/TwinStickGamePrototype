using ECS;
using Tags;
using UnityEngine;

[UpdateSystem]
public class DeleteDeadSystem : EcsSystem
{
    private int _filterId;

    public DeleteDeadSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<DeadTag>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            if (world.Have<Transform>(id))
                Object.Destroy(world.GetComponent<Transform>(id).gameObject);
            world.Delete(id);
        }
    }
}