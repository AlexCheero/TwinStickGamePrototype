using ECS;
using UnityEngine;

struct TestComponent { }

public class TestStartup : MonoBehaviour
{
    private EcsWorld _world;

    void Start()
    {
        _world = new EcsWorld();

        var filterId = _world.RegisterFilter(new BitMask(ComponentMeta<TestComponent>.Id));

        var entity = _world.Create();
        _world.AddComponent<TestComponent>(entity);

        _world.GetFilter(filterId).Iterate((entities, count) =>
        {
            int a = 0;
        });

        bool isDead = _world.IsDead(entity);

        _world.Delete(entity);

        isDead = _world.IsDead(entity);

        _world.GetFilter(filterId).Iterate((entities, count) =>
        {
            int a = 0;
        });
    }
}
