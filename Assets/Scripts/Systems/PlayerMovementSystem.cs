using ECS;
using UnityEngine;

sealed class PlayerMovementSystem : EcsSystem
{
    private int _camFilterId;
    private int _playerFilterId;

    public PlayerMovementSystem(EcsWorld world)
    {
        _camFilterId = world.RegisterFilter(new BitMask(Id<CameraTag>(), Id<Transform>()));
        _playerFilterId = world.RegisterFilter(new BitMask(Id<PlayerTag>(), Id<Transform>(), Id<SpeedComponent>()));
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

        world.GetFilter(_playerFilterId).Iterate((entities, count) =>
        {
            for (int i = 0; i < count; i++)
            {
                var transform = world.GetComponent<Transform>(entities[i]);
                moveDir *= Time.deltaTime * world.GetComponent<SpeedComponent>(entities[i]).speed;
                transform.Translate(moveDir, Space.World);
            }
        });
    }
}
