using Components;
using ECS;
using Tags;
using UnityEngine.SceneManagement;

//choose system type here
[System(ESystemCategory.LateFixedUpdate)]
public class LevelExitCollsionSystem : EcsSystem
{
    private readonly int _filterId;

    public LevelExitCollsionSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<LevelExit>(), Id<CollisionWith>()));
    }

    public override void Tick(EcsWorld world)
    {
        if (MapGoal.Instance.Goal != EGoal.CompleteLevel)
            return;

        foreach (var id in world.Enumerate(_filterId))
        {
            var collidedId = world.GetComponent<CollisionWith>(id).entity.GetId();
            if (world.Have<PlayerTag>(collidedId))
            {
                MiscUtils.AddScore(Constants.PointsForGoalCompletion);
                SceneManager.LoadScene(Constants.MainMenu);
            }    
        }
    }
}