using ECS;
using Tags;
using UnityEngine;

[ReactiveSystem(EReactionType.OnAdd, typeof(DeadTag))]
public static class OnDeadSystem
{
    private static BitMask _poolItemIncludes = new BitMask(ComponentMeta<PoolItem>.Id);
    private static BitMask _transformIncludes = new BitMask(ComponentMeta<Transform>.Id);

    public static void Tick(EcsWorld world, int id)
    {
        //must be checked before other Transforms
        if (world.CheckAgainstMasks(id, _poolItemIncludes))
        {

        }
        else if (world.CheckAgainstMasks(id, _transformIncludes))
            Object.Destroy(world.GetComponent<Transform>(id).gameObject);
        world.Delete(id);
    }
}
