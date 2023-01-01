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
            var attackPosition = transform.position + _view.GetEcsComponent<ViewOffset>().offset;
            _view.Add(new MeleeAttackEvent { position = attackPosition, direction = transform.forward });
        }
    }
}
