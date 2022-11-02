using Components;
using ECS;
using Tags;
using UnityEngine;

[System(ESystemCategory.Update)]
public class PlayerRotationSystem : EcsSystem
{
    private int _camFilterId;
    private int _playerFilterId;

    public PlayerRotationSystem(EcsWorld world)
    {
        _camFilterId = world.RegisterFilter(new BitMask(Id<CameraTag>(), Id<Camera>()));
        _playerFilterId = world.RegisterFilter(new BitMask(Id<PlayerTag>(), Id<PlayerDirectionComponent>(), Id<Transform>()));
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
            Entity weaponEntity = world.Have<CurrentWeapon>(id) ? world.GetComponent<CurrentWeapon>(id).entity : EntityExtension.NullEntity;
            bool isMoveRotation = world.Have<PlayerVelocityComponent>(id) && world.IsEntityValid(weaponEntity) &&
                                  world.Have<MeleeWeapon>(weaponEntity.GetId());
            if (isMoveRotation)
                MoveRotation(world, id);
            else
                TargetableRotation(world, id, ray);
        }
    }

    private void TargetableRotation(EcsWorld world, int id, Ray ray)
    {
        var transform = world.GetComponent<Transform>(id);
        var t = (transform.position.y - ray.origin.y) / ray.direction.y;
        var x = ray.origin.x + t * ray.direction.x;
        var z = ray.origin.z + t * ray.direction.z;

        var point = new Vector3(x, transform.position.y, z);
        //don't rotate if mouse points to player to prevent jerking
        if ((point - transform.position).sqrMagnitude < 0.3f)
            return;

        ref var direction = ref world.GetComponentByRef<PlayerDirectionComponent>(id).direction;
        direction = point - transform.position;
        direction.y = 0;
        direction.Normalize();
        transform.forward = direction;
    }

    private void MoveRotation(EcsWorld world, int id)
    {
        ref var direction = ref world.GetComponentByRef<PlayerDirectionComponent>(id).direction;
        var velocity = world.GetComponent<PlayerVelocityComponent>(id).velocity;
        if (velocity.sqrMagnitude > float.Epsilon)
            direction = velocity.normalized;
        world.GetComponent<Transform>(id).forward = direction;
    }
}
