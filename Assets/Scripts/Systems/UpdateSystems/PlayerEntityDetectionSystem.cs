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

    private const int CamHitsCount = 4;
    private readonly RaycastHit[] _camHitResults;
    
    public PlayerEntityDetectionSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<PlayerTag>(), Id<Transform>(), Id<ViewOffset>()));
        _camFilterId = world.RegisterFilter(new BitMask(Id<CameraTag>(), Id<Camera>()));

        _camHitResults = new RaycastHit[CamHitsCount];
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
            var start = transform.position + world.GetComponent<ViewOffset>(id).offset;
            var end = start;

            var camHitsCount = Physics.RaycastNonAlloc(cameraRay, _camHitResults);
            EntityView hittedEntity = null;
            Vector3 entityHitPoint = default;
            Vector3 targetHitNormal = default;
            for (int i = 0; i < camHitsCount; i++)
            {
                hittedEntity = _camHitResults[i].collider.GetComponent<EntityView>();
                entityHitPoint = _camHitResults[i].point;
                targetHitNormal = _camHitResults[i].normal;
                if (hittedEntity != null)
                    break;
            }
            
            ref var sight = ref world.GetComponent<PlayerSight>(id);
            if (hittedEntity != null && Physics.Raycast(start, entityHitPoint - start, out var hitInfo))
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
                if (Physics.Raycast(start, forward, out var hitInfo2))
                {
                    hittedEntity = hitInfo2.collider.GetComponent<EntityView>();
                    end = hitInfo2.point;
                }
            }
            
            sight.Start = start;
            sight.End = end;
            sight.Normal = targetHitNormal;
            sight.SightedView = hittedEntity;
        }
    }
}