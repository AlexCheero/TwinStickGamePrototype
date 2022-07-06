using ECS;
using Tags;
using UnityEngine;

[ReactiveSystem(EReactionType.OnAdd, typeof(DeadTag))]
public static class OnDeadSystem
{
    public static void Tick(EcsWorld world, int id)
    {
        if (world.Have<Transform>(id))
            Object.Destroy(world.GetComponent<Transform>(id).gameObject);
        world.Delete(id);
    }
}
