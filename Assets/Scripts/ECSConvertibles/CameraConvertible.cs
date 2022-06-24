using ECS;
using UnityEngine;

public class CameraConvertible : ECSConvertible
{
    [SerializeField]
    private Vector3 _direction;

    [SerializeField]
    private float _distance;

    protected override void AddComponents(EcsWorld world)
    {
        var playerFilterIdIncludes =
            new BitMask(
                ComponentMeta<PlayerTag>.Id,
                ComponentMeta<Transform>.Id
            );
        var playerFilterId = world.RegisterFilter(playerFilterIdIncludes);

        var id = Entity.GetId();
        world.AddTag<CameraTag>(id);
        world.AddComponent(id, transform);
        _direction.Normalize();
        world.AddComponent(id, new CameraSettingsComponent { direction = _direction, distance = _distance });
        world.AddComponent(id, GetComponent<Camera>());

        foreach (var entity in world.Enumerate(playerFilterId))
        {
            var targetTransform = world.GetComponent<Transform>(entity);
            world.AddComponent(id, new TargetTransformComponent { target = targetTransform });
            break;
        }
    }
}
