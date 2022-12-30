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

    private LineRenderer lpObject;
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
            var lpEnd = lpStart;
            
            lpObject ??= GameObject.FindObjectOfType<LineRenderer>();
            var color =
#if DEBUG
                Color.magenta;
#else
                Color.red;
#endif

            var hits = Physics.RaycastAll(ray);
            EntityView hittedEntity = null;
            Vector3 entityHitPoint = default;
            for (int i = 0; i < hits.Length; i++)
            {
                hittedEntity = hits[i].collider.GetComponent<EntityView>();
                entityHitPoint = hits[i].point;
                if (hittedEntity != null)
                    break;
            }
            
            if (hittedEntity != null)
            {
                ray = new Ray(lpStart, entityHitPoint - lpStart);
                Physics.Raycast(ray, out var hitInfo2);
                        
                lpEnd = hitInfo2.point;

                color = Color.green;
            }
            else
            {
                const float freeLpLength = 1000;
                var forward = transform.forward;
                lpEnd = lpStart + forward * freeLpLength;

                ray = new Ray(lpStart, forward);
                if (Physics.Raycast(ray, out var hitInfo2))
                    lpEnd = hitInfo2.point;
                
                color = Color.red;
            }
            
            lpObject.SetPosition(0, lpStart);
            lpObject.SetPosition(1, lpEnd);
            lpObject.startColor = color;
            lpObject.endColor = color;
        }
    }
}