using System;
using UnityEngine;

[RequireComponent(typeof(EntityView))]
public class PooledEntityView : PoolItem
{
    private EntityView _view;

    private void Start() => _view = GetComponent<EntityView>();

    public override void ReturnToPool()
    {
        base.ReturnToPool();
        _view.DeleteFromWorld();
    }
}
