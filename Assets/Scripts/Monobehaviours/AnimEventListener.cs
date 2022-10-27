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
            var rightPalm = MiscUtils.FindGrandChildByName(transform, "RightPalm");
            _view.Add(new GrenadeFlyEvent { position = rightPalm.position, direction = transform.forward });
        }
    }

    void MeleeAttack(string from)
    {
        if (!_view.Have<MeleeAttackEvent>())
        {
            var pos = transform.position;
            var attackFrom = MiscUtils.FindGrandChildByNamePart(transform, from);
            if (attackFrom != null)
                pos.y = attackFrom.position.y;
            _view.Add(new MeleeAttackEvent { position = pos, direction = transform.forward });
        }
    }
}
