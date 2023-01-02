using Components;
using ECS;
using Tags;
using UnityEngine;

[System(ESystemCategory.LateUpdate)]
public class CameraFollowSystem : EcsSystem
{
    private int _filterId;

    public CameraFollowSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<CameraTag>(), Id<Transform>(), Id<TargetEntityComponent>(),
            Id<CameraSettingsComponent>()));
    }

    //TODO: implement smoothing
    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            var cameraSettings = world.GetComponent<CameraSettingsComponent>(id);
            var transform = world.GetComponent<Transform>(id);
            var targetTransform = world.GetComponent<TargetEntityComponent>(id).target.transform;

            var newPosition = targetTransform.position;
            newPosition += cameraSettings.direction * cameraSettings.distance;
            transform.position = newPosition;
            transform.LookAt(targetTransform);
        }
    }
}
