using Components;
using ECS;
using Tags;
using UnityEngine;
using UnityEngine.SceneManagement;

//choose system type here
[System(ESystemCategory.LateFixedUpdate)]
public class LevelExitCollisionSystem : EcsSystem
{
    private readonly int _goalFilterId;
    private readonly int _filterId;
    private readonly int _levelSettingsFilterId;

    public LevelExitCollisionSystem(EcsWorld world)
    {
        _goalFilterId = world.RegisterFilter(new BitMask(Id<LevelGoal>()));
        _filterId = world.RegisterFilter(new BitMask(Id<LevelExit>(), Id<CollisionWith>()));
        _levelSettingsFilterId = world.RegisterFilter(new BitMask(Id<LevelSettingsComponent>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_goalFilterId))
        {
            if (world.GetComponent<LevelGoal>(id).goal != EGoal.CompleteLevel)
                return;
        }

        int levelSettingsId = -1;
        foreach (var id in world.Enumerate(_levelSettingsFilterId))
        {
            levelSettingsId = id;
            break;
        }

        foreach (var id in world.Enumerate(_filterId))
        {
            var collidedId = world.GetComponent<CollisionWith>(id).entity.GetId();
            if (!world.Have<PlayerTag>(collidedId))
            {
                Debug.Log("something collided level exit");
                continue;
            }
            
            if (levelSettingsId < 0)
            {
                Debug.LogError("level settings not found");
                return;
            }
            
            MiscUtils.AddScore(Constants.PointsForGoalCompletion);
            SceneManager.LoadScene("ProceduralLevel");
            break;
        }
    }
}