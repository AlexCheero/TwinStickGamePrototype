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
        if (!_view.Have<GrenadeFly>())
        {
            var gunHolder = MiscUtils.FindGrandChildByName(transform, "GunHolder");
            _view.Add(new GrenadeFly { position = gunHolder.position, direction = transform.forward });
        }
    }
}
