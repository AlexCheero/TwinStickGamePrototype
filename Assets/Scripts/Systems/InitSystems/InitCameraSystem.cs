using Components;
using ECS;
using Tags;

[System(ESystemCategory.Init)]
public class InitCameraSystem : EcsSystem
{
    private int _camFilterId;

    public InitCameraSystem(EcsWorld world)
    {
        _camFilterId = world.RegisterFilter(new BitMask(Id<CameraTag>(), Id<CameraSettingsComponent>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_camFilterId))
            world.GetComponent<CameraSettingsComponent>(id).direction.Normalize();
    }
}
