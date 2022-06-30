using Components;
using ECS;
using Tags;
using UnityEngine;

//TODO: try to make it more generic
[RequireComponent(typeof(EntityView))]
public class ProjectileCollision : MonoBehaviour
{
    private EntityView _view;

    public int InitAsEntity(EcsWorld world)
    {
        _view = GetComponent<EntityView>();
        var id = _view.InitAsEntity(world);

#if UNITY_EDITOR
        if (!_view.Have<Projectile>())
            throw new System.Exception("entity have no needed component");
#endif

        return id;
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Projectile collided");
        var collidedView = collision.gameObject.GetComponent<EntityView>();
        if (collidedView != null)
        {
            if (collidedView.Have<HealthComponent>())
            {
                Debug.Log("Projectile damaged!");
                collidedView.GetEcsComponent<HealthComponent>().health -= _view.GetEcsComponent<DamageComponent>().damage;
            }
        }

        _view.DeleteSelf();
    }
}
