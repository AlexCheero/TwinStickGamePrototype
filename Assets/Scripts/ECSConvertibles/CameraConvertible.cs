using ECS;
using UnityEngine;

public class CameraConvertible : ECSConvertible
{
    [SerializeField]
    private Vector3 _direction;

    [SerializeField]
    private float _distance;

    public override void ConvertToEntity(EcsWorld world)
    {
        var playerFilterIdIncludes =
            new BitMask(
                ComponentMeta<PlayerTag>.Id,
                ComponentMeta<Transform>.Id
            );
        var playerFilterId = world.RegisterFilter(playerFilterIdIncludes);

        var cameraEntity = world.Create();
        world.AddTag<CameraTag>(cameraEntity);
        world.AddComponent(cameraEntity, transform);
        _direction.Normalize();
        world.AddComponent(cameraEntity, new CameraSettingsComponent { direction = _direction, distance = _distance });
        world.AddComponent(cameraEntity, GetComponent<Camera>());

        world.GetFilter(playerFilterId).Iterate((entities, count) =>
        {
            var targetTransform = world.GetComponent<Transform>(entities[0]);
            world.AddComponent(cameraEntity, new TargetTransformComponent { target = targetTransform });
        });
    }
}
