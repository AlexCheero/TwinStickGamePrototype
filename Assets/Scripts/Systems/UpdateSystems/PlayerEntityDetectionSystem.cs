using Components;
using ECS;
using Tags;
using UnityEngine;

//choose system type here
[System(ESystemCategory.Update)]
public class PlayerEntityDetectionSystem : EcsSystem
{
    private readonly int _filterId;
    private readonly int _camFilterId;

    public PlayerEntityDetectionSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<PlayerTag>(), Id<Transform>(), Id<Collider>()));
        _camFilterId = world.RegisterFilter(new BitMask(Id<CameraTag>(), Id<Camera>()));
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

        var cameraRay = cam.ScreenPointToRay(Input.mousePosition);
        
        foreach (var id in world.Enumerate(_filterId))
        {
            var transform = world.GetComponent<Transform>(id);
            
            var bounds = world.GetComponent<Collider>(id).bounds;
            var start = bounds.center;
            //3/4 upper part of collider
            start.y += bounds.extents.y / 2;
            var end = start;

            var hits = Physics.RaycastAll(cameraRay);
            EntityView hittedEntity = null;
            Vector3 entityHitPoint = default;
            Vector3 targetHitNormal = default;
            for (int i = 0; i < hits.Length; i++)
            {
                hittedEntity = hits[i].collider.GetComponent<EntityView>();
                entityHitPoint = hits[i].point;
                targetHitNormal = hits[i].normal;
                if (hittedEntity != null)
                    break;
            }
            
            ref var sight = ref world.GetOrAddComponent<PlayerSight>(id);
            if (hittedEntity != null && Physics.Raycast(new Ray(start, entityHitPoint - start), out var hitInfo))
            {
                hittedEntity = hitInfo.collider.GetComponent<EntityView>();
                end = hitInfo.point;
            }
            else
            {
                hittedEntity = null;
                const float freeLpLength = 1000;
                var forward = transform.forward;
                end = start + forward * freeLpLength;
                var ray = new Ray(start, forward);
                if (Physics.Raycast(ray, out var hitInfo2))
                    end = hitInfo2.point;
            }
            
            sight.Start = start;
            sight.End = end;
            sight.Normal = targetHitNormal;
            sight.SightedView = hittedEntity;
        }
    }
}