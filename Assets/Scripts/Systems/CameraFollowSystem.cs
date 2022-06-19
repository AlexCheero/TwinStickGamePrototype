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
        world.GetFilter(_filterId).Iterate((entities, count) =>
        {
            for (int i = 0; i < count; i++)
            {
                var cameraSettings = world.GetComponent<CameraSettingsComponent>(entities[i]);
                var transform = world.GetComponent<Transform>(entities[i]);
                var targetTransform = world.GetComponent<TargetTransformComponent>(entities[i]).target;

                var newPosition = targetTransform.position;
                newPosition += cameraSettings.direction * cameraSettings.distance;
                transform.position = newPosition;
                transform.LookAt(targetTransform);
            }
        });
    }
}
