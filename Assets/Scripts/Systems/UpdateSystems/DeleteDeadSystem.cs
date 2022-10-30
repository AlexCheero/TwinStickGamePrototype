using ECS;
using Tags;
using UnityEngine;

[System(ESystemCategory.LateUpdate, ESystemCategory.LateFixedUpdate)]
public class DeleteDeadSystem : EcsSystem
{
    private int _poolItemfilterId;
    private int _poolItemRBfilterId;
    private int _transformfilterId;
    private int _deadEntitiesFilterId;

    public DeleteDeadSystem(EcsWorld world)
    {
        _poolItemfilterId = world.RegisterFilter(new BitMask(Id<DeadTag>(), Id<PoolItem>()), new BitMask(Id<Rigidbody>()));
        _poolItemRBfilterId = world.RegisterFilter(new BitMask(Id<DeadTag>(), Id<PoolItem>(), Id<Rigidbody>()));
        _transformfilterId = world.RegisterFilter(new BitMask(Id<DeadTag>(), Id<Transform>()), new BitMask(Id<PoolItem>()));
        _deadEntitiesFilterId = world.RegisterFilter(new BitMask(Id<DeadTag>()), new BitMask(Id<Transform>(), Id<PoolItem>()));
    }

    /*
     * ReturnToPool also deletes entity if it is the PooledEntityView
     */
    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_poolItemfilterId))
        {
            world.GetComponent<PoolItem>(id).ReturnToPool();
        }

        foreach (var id in world.Enumerate(_poolItemRBfilterId))
        {
            world.GetComponent<Rigidbody>(id).velocity = Vector3.zero;
            world.GetComponent<PoolItem>(id).ReturnToPool();
        }

        foreach (var id in world.Enumerate(_transformfilterId))
        {
            var gameObject = world.GetComponent<Transform>(id).gameObject;
            foreach (var poolItem in gameObject.GetComponentsInChildren<PoolItem>())
                poolItem.ReturnToPool();
            Object.Destroy(gameObject);
            world.Delete(id);
        }

        foreach (var id in world.Enumerate(_deadEntitiesFilterId))
        {
            world.Delete(id);
        }
    }
}