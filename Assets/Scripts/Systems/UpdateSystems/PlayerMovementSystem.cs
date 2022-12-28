using Components;
using ECS;
using Tags;
using UnityEngine;

[System(ESystemCategory.Update)]
sealed class PlayerMovementSystem : EcsSystem
{
    private int _camFilterId;
    private int _playerFilterId;

    public PlayerMovementSystem(EcsWorld world)
    {
        _camFilterId = world.RegisterFilter(new BitMask(Id<CameraTag>(), Id<Transform>()));
        _playerFilterId = world.RegisterFilter(new BitMask(Id<PlayerTag>(), Id<SpeedComponent>(), Id<PlayerVelocityComponent>()));
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

        Vector3 camForwardFlat = cam.transform.forward;
        camForwardFlat.y = 0;
        camForwardFlat.Normalize();

        Vector3 camRightFlat = cam.transform.right;
        camRightFlat.y = 0;
        camRightFlat.Normalize();

        //TODO: use new input system package and analogue axes
        var moveDir = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
            moveDir += camForwardFlat;
        if (Input.GetKey(KeyCode.S))
            moveDir -= camForwardFlat;
        if (Input.GetKey(KeyCode.D))
            moveDir += camRightFlat;
        if (Input.GetKey(KeyCode.A))
            moveDir -= camRightFlat;
        moveDir.Normalize();

        foreach (var id in world.Enumerate(_playerFilterId))
        {
            world.GetComponent<PlayerVelocityComponent>(id).velocity =
                moveDir * world.GetComponent<SpeedComponent>(id).speed;
        }
    }
}
