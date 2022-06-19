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
        world.GetFilter(_camFilterId).Iterate((entities, count) =>
            cam = world.GetComponent<Camera>(entities[0]));

#if DEBUG
        if (cam == null)
            throw new System.Exception("can't find camera entity");
#endif

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (!Physics.Raycast(ray, out hit, 100))
            return;

        world.GetFilter(_playerFilterId).Iterate((entities, count) =>
        {
            for (int i = 0; i < count; i++)
            {
                var transform = world.GetComponent<Transform>(entities[i]);
                var direction = hit.point - transform.position;
                direction.y = 0;
                direction.Normalize();
                transform.rotation = Quaternion.LookRotation(direction);
            }
        });
    }
}
