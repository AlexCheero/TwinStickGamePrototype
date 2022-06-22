using ECS;
using UnityEngine;

public class CameraFollowSystem : EcsSystem
{
    private int _filterId;

    public CameraFollowSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<CameraTag>(), Id<Transform>(), Id<TargetTransformComponent>(),
            Id<CameraSettingsComponent>()));
    }

    //TODO: implement smoothing
    public override void Tick(EcsWorld world)
    {
        foreach (var entity in world.Enumerate(_filterId))
        {
            var cameraSettings = world.GetComponent<CameraSettingsComponent>(entity);
            var transform = world.GetComponent<Transform>(entity);
            var targetTransform = world.GetComponent<TargetTransformComponent>(entity).target;

            if (targetTransform == null)
            {
                world.RemoveComponent<TargetTransformComponent>(entity);
                continue;
            }

            var newPosition = targetTransform.position;
            newPosition += cameraSettings.direction * cameraSettings.distance;
            transform.position = newPosition;
            transform.LookAt(targetTransform);
        }
    }
}
