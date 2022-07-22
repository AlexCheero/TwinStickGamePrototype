using ECS;
using UnityEngine;

//choose system type here
[System(ESystemCategory.Init)]
public class InitEntityViewPoolsSystem : EcsSystem
{
    public InitEntityViewPoolsSystem(EcsWorld _) {}

    public override void Tick(EcsWorld world)
    {
        var pools = Object.FindObjectsOfType<ObjectPool>();
        foreach (var pool in pools)
        {
            var viewPrototype = pool.GetPrototype<EntityView>();
            if (viewPrototype != null)
                viewPrototype.InitAsEntity(world);
        }
    }
}