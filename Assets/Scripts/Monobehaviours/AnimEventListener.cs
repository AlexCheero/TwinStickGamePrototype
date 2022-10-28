using Components;
using UnityEngine;

[RequireComponent(typeof(EntityView))]
public class AnimEventListener : MonoBehaviour
{
    private EntityView _view;

    void Start()
    {
        _view = GetComponent<EntityView>();
    }

    void GrenadeFly()
    {
        if (!_view.Have<GrenadeFlyEvent>())
        {
            var gunHolder = MiscUtils.FindGrandChildByName(transform, "GunHolder");
            _view.Add(new GrenadeFlyEvent { position = gunHolder.position, direction = transform.forward });
        }
    }

    void MeleeAttack()
    {
        if (!_view.Have<MeleeAttackEvent>())
        {
            Vector3 attackPosition;
            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                var bounds = collider.bounds;
                attackPosition = bounds.center;
                //3/4 upper part of collider
                attackPosition.y += bounds.extents.y / 2;
            }
            else
            {
                attackPosition = transform.position;
            }
            
            _view.Add(new MeleeAttackEvent { position = attackPosition, direction = transform.forward });
        }
    }
}
