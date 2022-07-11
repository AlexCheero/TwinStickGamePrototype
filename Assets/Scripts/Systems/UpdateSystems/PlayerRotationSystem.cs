using ECS;
using Tags;
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
        foreach (var id in world.Enumerate(_camFilterId))
        {
            cam = world.GetComponent<Camera>(id);
            break;
        }

#if DEBUG
        if (cam == null)
            throw new System.Exception("can't find camera entity");
#endif

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        foreach (var id in world.Enumerate(_playerFilterId))
        {
            var transform = world.GetComponent<Transform>(id);
            var t = (transform.position.y - ray.origin.y) / ray.direction.y;
            var x = ray.origin.x + t * ray.direction.x;
            var z = ray.origin.z + t * ray.direction.z;

            var point = new Vector3(x, transform.position.y, z);
            //don't rotate if mouse points to player to prevent jerking
            if ((point - transform.position).sqrMagnitude < 0.3f)
                continue;

            var direction = point - transform.position;
            direction.y = 0;
            direction.Normalize();
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}
