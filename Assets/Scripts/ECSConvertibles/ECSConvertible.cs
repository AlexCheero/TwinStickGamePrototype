using ECS;
using UnityEngine;

public abstract class ECSConvertible : MonoBehaviour
{
    public Entity Entity { get; private set; }
    protected EcsWorld _world;

    private void CreateEntity(EcsWorld world)// => Entity = world.GetById(world.Create());
    {
        var id = world.Create();
        Entity = world.GetById(id);
    }

    protected abstract void AddComponents(EcsWorld world);

    public int ConvertToEntity(EcsWorld world)
    {
        _world = world;
        CreateEntity(world);
        AddComponents(world);
        return Entity.GetId();
    }
}
