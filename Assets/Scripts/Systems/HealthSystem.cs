using ECS;
using System.Collections.Generic;
using UnityEngine;

public class HealthSystem : EcsSystem
{
    private int _filterId;
    //TODO: remove this hack and implement iterators on world/filter with proper locking
    private HashSet<int> _deletList;

    public HealthSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<HealthComponent>()));
        _deletList = new HashSet<int>();
    }

    public override void Tick(EcsWorld world)
    {
        world.GetFilter(_filterId).Iterate((entities, count) =>
        {
            for (int i = 0; i < count; i++)
            {
                var health = world.GetComponent<HealthComponent>(entities[i]).health;
                if (health > 0)
                    continue;

                if (world.Have<Transform>(entities[i]))
                    Object.Destroy(world.GetComponent<Transform>(entities[i]).gameObject);
                //world.Delete(entities[i]);
                _deletList.Add(entities[i]);
            }
        });

        foreach (var i in _deletList)
            world.Delete(i);
        _deletList.Clear();
    }
}
