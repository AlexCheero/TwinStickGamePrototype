using ECS;
using UnityEngine;

public class PlayerRotationSystem : EcsSystem
{
    private int _camFilterId;
    private int _playerFilterId;

    public PlayerRotationSystem(EcsWorld world)
    {
        _camFilterId = world.RegisterFilter(new BitMask(Id<CameraTag>(), Id<Camera>()));
        _playerFilterId = world.RegisterFilter(new BitMask(Id<PlayerTag>(), Id<Transform>()));
    }

    public override void Tick(EcsWorld world)
    {
        //TODO: cache
        Camera cam = null;
        foreach (var entity in world.Enumerate(_camFilterId))
        {
            cam = world.GetComponent<Camera>(entity);
            break;
        }

#if DEBUG
        if (cam == null)
            throw new System.Exception("can't find camera entity");
#endif

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        //TODO: remove physics and use point with same height as player
        if (!Physics.Raycast(ray, out hit, 100))
            return;

        foreach (var entity in world.Enumerate(_playerFilterId))
        {
            var transform = world.GetComponent<Transform>(entity);
            var direction = hit.point - transform.position;
            direction.y = 0;
            direction.Normalize();
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}
