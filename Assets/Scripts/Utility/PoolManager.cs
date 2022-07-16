
using System.Collections.Generic;
using UnityEngine;

public static class PoolManager
{
    private static Dictionary<string, ObjectPool> _pools = new Dictionary<string, ObjectPool>();

    public static ObjectPool Get(string name)
    {
        if (!_pools.ContainsKey(name))
        {
            var pool = GameObject.Find(name).GetComponent<ObjectPool>();
            _pools.Add(name, pool);
        }

        return _pools[name];
    }
}
