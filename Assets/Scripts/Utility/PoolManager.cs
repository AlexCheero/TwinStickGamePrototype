
using System.Collections.Generic;
using UnityEngine;

public static class PoolManager
{
    private static readonly Dictionary<string, ObjectPool> _pools = new Dictionary<string, ObjectPool>();

    public static ObjectPool Get(string name)
    {
        if (!_pools.ContainsKey(name) || _pools[name] == null)
        {
            var pool = GameObject.Find(name).GetComponent<ObjectPool>();
            _pools[name] = pool;
        }

        return _pools[name];
    }
}
