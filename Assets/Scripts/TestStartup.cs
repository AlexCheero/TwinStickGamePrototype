using ECS;
using UnityEngine;
using UnityEngine.Profiling;

public struct IntComp { public int i; }
public struct FloatComp { public float f; }
public struct AddTag { }
public struct SubtractTag { }

public abstract class BaseTestSystem : EcsSystem
{
    public void DoHeavyStuff()
    {
        for (int i = 0; i < 10; i++)
        {
            var a = new Vector3(Random.Range(0, 100), Random.Range(0, 100), Random.Range(0, 100));
            var b = new Vector3(Random.Range(0, 100), Random.Range(0, 100), Random.Range(0, 100));
            var mgntd = (a - b).magnitude;
            var normlzd = (a - b).normalized;
        }
    }
}

public class System1 : BaseTestSystem
{
    int _fltrId;

    public System1(EcsWorld world)
    {
        _fltrId = world.RegisterFilter(new BitMask(Id<AddTag>(), Id<IntComp>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var entity in world.Enumerate(_fltrId))
        {
            world.GetComponent<IntComp>(entity).i += 1;
            if (world.Have<FloatComp>(entity))
                world.GetComponent<FloatComp>(entity).f += 0.1f;
            DoHeavyStuff();
        }
    }
}

public class System2 : BaseTestSystem
{
    int _fltrId;

    public System2(EcsWorld world)
    {
        _fltrId = world.RegisterFilter(new BitMask(Id<SubtractTag>(), Id<FloatComp>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var entity in world.Enumerate(_fltrId))
        {
            world.GetComponent<FloatComp>(entity).f -= 0.1f;
            if (world.Have<IntComp>(entity))
                world.GetComponent<IntComp>(entity).i -= 1;
            DoHeavyStuff();
        }
    }
}

public class System3 : BaseTestSystem
{
    int _fltrId1;
    int _fltrId2;

    public System3(EcsWorld world)
    {
        _fltrId1 = world.RegisterFilter(new BitMask(Id<AddTag>()));
        _fltrId2 = world.RegisterFilter(new BitMask(Id<SubtractTag>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var entity in world.Enumerate(_fltrId1))
        {
            if (world.Have<IntComp>(entity) && world.GetComponent<IntComp>(entity).i > 1000)
            {
                world.RemoveComponent<AddTag>(entity);
                world.AddTag<SubtractTag>(entity);
            }
            DoHeavyStuff();
        }

        foreach (var entity in world.Enumerate(_fltrId2))
        {
            if (world.Have<FloatComp>(entity) && world.GetComponent<FloatComp>(entity).f <= 0)
            {
                world.RemoveComponent<SubtractTag>(entity);
                world.AddTag<AddTag>(entity);
            }
            DoHeavyStuff();
        }
    }
}

public class System1_old : BaseTestSystem
{
    int _fltrId;

    public System1_old(EcsWorld world)
    {
        _fltrId = world.RegisterFilter(new BitMask(Id<AddTag>(), Id<IntComp>()));
    }

    public override void Tick(EcsWorld world)
    {
        world.GetFilter(_fltrId).Iterate((entities, count) =>
        {
            for (int i = 0; i < count; i++)
            {
                world.GetComponent<IntComp>(entities[i]).i += 1;
                if (world.Have<FloatComp>(entities[i]))
                    world.GetComponent<FloatComp>(entities[i]).f += 0.1f;
                DoHeavyStuff();
            }
        });
    }
}

public class System2_old : BaseTestSystem
{
    int _fltrId;

    public System2_old(EcsWorld world)
    {
        _fltrId = world.RegisterFilter(new BitMask(Id<SubtractTag>(), Id<FloatComp>()));
    }

    public override void Tick(EcsWorld world)
    {
        world.GetFilter(_fltrId).Iterate((entities, count) =>
        {
            for (int i = 0; i < count; i++)
            {
                world.GetComponent<FloatComp>(entities[i]).f -= 0.1f;
                if (world.Have<IntComp>(entities[i]))
                    world.GetComponent<IntComp>(entities[i]).i -= 1;
                DoHeavyStuff();
            }
        });
    }
}

public class System3_old : BaseTestSystem
{
    int _fltrId1;
    int _fltrId2;

    public System3_old(EcsWorld world)
    {
        _fltrId1 = world.RegisterFilter(new BitMask(Id<AddTag>()));
        _fltrId2 = world.RegisterFilter(new BitMask(Id<SubtractTag>()));
    }

    public override void Tick(EcsWorld world)
    {
        world.GetFilter(_fltrId1).Iterate((entities, count) =>
        {
            for (int i = 0; i < count; i++)
            {
                if (world.Have<IntComp>(entities[i]) && world.GetComponent<IntComp>(entities[i]).i > 1000)
                {
                    world.RemoveComponent<AddTag>(entities[i]);
                    world.AddTag<SubtractTag>(entities[i]);
                }
                DoHeavyStuff();
            }
        });

        world.GetFilter(_fltrId2).Iterate((entities, count) =>
        {
            for (int i = 0; i < count; i++)
            {
                if (world.Have<FloatComp>(entities[i]) && world.GetComponent<FloatComp>(entities[i]).f <= 0)
                {
                    world.RemoveComponent<SubtractTag>(entities[i]);
                    world.AddTag<AddTag>(entities[i]);
                }
                DoHeavyStuff();
            }
        });
    }
}

public class TestStartup : MonoBehaviour
{
    public bool old = false;
    private EcsWorld _world;
    private EcsSystem[] _updateSystems;

    void Start()
    {
        _world = new EcsWorld();
        if (old)
        {
            _updateSystems = new EcsSystem[]
            {
              new System1_old(_world),
              new System2_old(_world),
              new System3_old(_world)
            };
        }
        else
        {
            _updateSystems = new EcsSystem[]
            {
              new System1(_world),
              new System2(_world),
              new System3(_world)
            };
        }

        for (int i = 0; i < 300; i++)
        {
            var entity = _world.Create();

            var rem = i % 6;

            if (rem == 0)
            {
                _world.AddTag<AddTag>(entity);
                _world.AddComponent<IntComp>(entity);
            }
            else if (rem == 1)
            {
                _world.AddTag<AddTag>(entity);
                _world.AddComponent<FloatComp>(entity);
            }
            else if (rem == 2)
            {
                _world.AddTag<AddTag>(entity);
                _world.AddComponent<IntComp>(entity);
                _world.AddComponent<FloatComp>(entity);
            }

            else if (rem == 3)
            {
                _world.AddTag<SubtractTag>(entity);
                _world.AddComponent(entity, new IntComp { i = 1000 });
            }
            else if (rem == 4)
            {
                _world.AddTag<SubtractTag>(entity);
                _world.AddComponent(entity, new FloatComp { f = 100 });
            }
            else if (rem == 5)
            {
                _world.AddTag<SubtractTag>(entity);
                _world.AddComponent(entity, new IntComp { i = 1000 });
                _world.AddComponent(entity, new FloatComp { f = 100 });
            }
        }
    }

    void Update()
    {
        Profiler.BeginSample("Ecs world tick");
        foreach (var system in _updateSystems)
            system.Tick(_world);
        Profiler.EndSample();
    }
}
