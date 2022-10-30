using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//TODO: try to implement generic pools, that holds components of proper types
public class ObjectPool : MonoBehaviour
{
    [SerializeField]
    private int _initialCount;
    [SerializeField]
    private PoolItem _prototype;
    [SerializeField]
    private PoolItem[] _objects;
    private int _firstAvailable = 0;

    private Dictionary<Type, MonoBehaviour> _cachedPrototypes;

#if UNITY_EDITOR
    [MenuItem("Pools/Fix pools", false, -1)]
    private static void FixPools()
    {
        foreach (var pool in FindObjectsOfType<ObjectPool>())
            FixPool(pool);
    }

    private static void FixPool(ObjectPool pool)
    {
        if (pool._initialCount == 0 || (pool._initialCount & (pool._initialCount - 1)) != 0)
        {
            Debug.LogError("pool " + pool.name + " size should be power of two");
            return;
        }

        //make sure that there are will be only copies of prototype
        var childCount = pool.transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            var childObj = pool.transform.GetChild(i).gameObject;
            DestroyImmediate(childObj);
        }

        Array.Resize(ref pool._objects, pool._initialCount);

        for (int i = 0; i < pool._initialCount; i++)
            pool.AddNew(i);
        EditorUtility.SetDirty(pool);
    }
#endif

    void Awake()
    {
        _cachedPrototypes = new Dictionary<Type, MonoBehaviour>();
    }

    public T GetPrototype<T>() where T : MonoBehaviour
    {
        var type = typeof(T);
        if (!_cachedPrototypes.ContainsKey(type))
        {
            T monoBeh = _prototype.GetComponent<T>();
#if DEBUG
            if (monoBeh == null)
                return null;
#endif
            _cachedPrototypes.Add(type, monoBeh);
        }
        return _cachedPrototypes[type] as T;
    }

    public T Get<T>() where T : Component => Get<T>(Vector3.zero, Quaternion.identity);
    public T Get<T>(Vector3 position) where T : Component => Get<T>(position, Quaternion.identity);

    public T Get<T>(Vector3 position, Quaternion rotation) where T : Component
        => GetObject(position, rotation).GetComponent<T>();

    public GameObject GetObject() => GetObject(Vector3.zero, Quaternion.identity);
    public GameObject GetObject(Vector3 position) => GetObject(position, Quaternion.identity);
    public GameObject GetObject(Vector3 position, Quaternion rotation)
    {
#if DEBUG
        if (_firstAvailable > _objects.Length)
            throw new Exception("_firstAvailable can't be bigger than _objects.Length");
#endif
        if (_firstAvailable == _objects.Length)
            Grow();

        var obj = _objects[_firstAvailable];
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.gameObject.SetActive(true);
        _firstAvailable++;
        return obj.gameObject;
    }

    private void Grow()
    {
        var newLength = _objects.Length << 1;
        Array.Resize(ref _objects, newLength);

        for (int i = _firstAvailable; i < _objects.Length; i++)
        {
#if DEBUG
            if (_objects[i] != null)
                throw new Exception("non null pool items after grow");
#endif
            AddNew(i);
        }
    }

    private void AddNew(int idx)
    {
        _objects[idx] = Instantiate(_prototype, transform);
        _objects[idx].AddToPool(this, idx);
        _objects[idx].gameObject.SetActive(false);
    }

    public void ReturnItem(PoolItem item)
    {
#if DEBUG
        //TODO: check if items is copy of _prototype

        if (_firstAvailable == 0)
            throw new Exception("pool have no active items but something is returned");
#endif
        item.gameObject.SetActive(false);
        item.transform.parent = transform;
        item.transform.position = Vector3.zero;
        item.transform.rotation = Quaternion.identity;

        _firstAvailable--;
        if (item.Idx < _firstAvailable)
        {
            var temp = _objects[_firstAvailable];
            _objects[_firstAvailable] = item;
            _objects[item.Idx] = temp;
            item.AddToPool(this, _firstAvailable);
        }
    }
}
