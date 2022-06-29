using Components;
using ECS;
using Tags;
using UnityEngine;

public class InitCameraSystem : EcsSystem
{
    private int _camFilterId;
    private int _playerFilterId;

    public InitCameraSystem(EcsWorld world)
    {
        _camFilterId = world.RegisterFilter(new BitMask(Id<CameraTag>(), Id<CameraSettingsComponent>()));
        _playerFilterId = world.RegisterFilter(new BitMask(Id<PlayerTag>(), Id<Transform>()));
    }

    public override void Tick(EcsWorld world)
    {
        int camId = -1;
        foreach (var id in world.Enumerate(_camFilterId))
        {
            world.GetComponent<CameraSettingsComponent>(id).direction.Normalize();
            camId = id;
            break;
        }

#if UNITY_EDITOR
        if (camId < 0)
            throw new System.Exception("can't find camera entity");
#endif

        foreach (var entity in world.Enumerate(_playerFilterId))
        {
            var targetTransform = world.GetComponent<Transform>(entity);
            world.AddComponent(camId, new TargetTransformComponent { target = targetTransform });
            break;
        }
    }
}
