using UnityEngine;

public class PoolItem : MonoBehaviour
{
    [SerializeField]
    private ObjectPool _pool;
    [SerializeField]
    private int _idx;

    public int Idx => _idx;

    public void AddToPool(ObjectPool pool, int idx)
    {
        _pool = pool;
        _idx = idx;
    }

    public virtual void ReturnToPool() => _pool.ReturnItem(this);
}
