using UnityEngine;

public class PoolItem : MonoBehaviour
{
    [SerializeField]
    private ObjectPool _pool;

    public ObjectPool Pool { get => _pool; set => _pool = value; }

    public void ReturnToPool()
    {
        _pool.ReturnItem(this);
    }
}
