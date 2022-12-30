using ECS;
using Tags;
using UnityEngine;

//choose system type here
[System(ESystemCategory.Update)]
public class LaserPointerSystem : EcsSystem
{
    private readonly int _filterId;
    private readonly int _camFilterId;

    public LaserPointerSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<PlayerTag>(), Id<Transform>(), Id<Collider>()));
        _camFilterId = world.RegisterFilter(new BitMask(Id<CameraTag>(), Id<Camera>()));
    }

    private LineRenderer lr;
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
        
        foreach (var id in world.Enumerate(_filterId))
        {
            var transform = world.GetComponent<Transform>(id);
            
            var bounds = world.GetComponent<Collider>(id).bounds;
            var lpStart = bounds.center;
            //3/4 upper part of collider
            lpStart.y += bounds.extents.y / 2;
            lr ??= GameObject.FindObjectOfType<LineRenderer>();
            var green = Color.green;
            var red = Color.red;
            var isCastHit = Physics.Raycast(ray, out var hitInfo);
            if (isCastHit && hitInfo.collider.GetComponent<EntityView>() != null)
            {
                ray = new Ray(lpStart, hitInfo.point - lpStart);
                Physics.Raycast(ray, out var hitInfo2);
                        
                var lpEnd = hitInfo2.point;

                lr.SetPosition(0, lpStart);
                lr.SetPosition(1, lpEnd);
                lr.startColor = green;
                lr.endColor = green;
            }
            else
            {
                const float freeLpLength = 1000;
                var lpEnd = lpStart + transform.forward * freeLpLength;

                ray = new Ray(lpStart, transform.forward);
                if (Physics.Raycast(ray, out var hitInfo2))
                    lpEnd = hitInfo2.point;
                
                lr.SetPosition(0, lpStart);
                lr.SetPosition(1, lpEnd);
                
                lr.startColor = red;
                lr.endColor = red;
            }
        }
    }
}