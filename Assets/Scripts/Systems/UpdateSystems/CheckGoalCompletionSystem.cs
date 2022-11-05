using Components;
using ECS;
using Tags;
using UnityEngine.SceneManagement;

//choose system type here
[System(ESystemCategory.Update)]
public class CheckGoalCompletionSystem : EcsSystem
{
    private readonly int _goalFilterId;
    private readonly int _killAllFilterId;

    public CheckGoalCompletionSystem(EcsWorld world)
    {
        _goalFilterId = world.RegisterFilter(new BitMask(Id<LevelGoal>()));
        _killAllFilterId = world.RegisterFilter(new BitMask(Id<EnemyTag>()), new BitMask(Id<DeadTag>()));
    }

    public override void Tick(EcsWorld world)
    {
        var isGoalComplete = false;
        var goal = EGoal.KillAll;
        foreach (var id in world.Enumerate(_goalFilterId))
        {
            goal = world.GetComponent<LevelGoal>(id).goal;
            break;
        }
        
        if (goal == EGoal.KillAll)
        {
            //TODO: implement count property for filters
            bool haveEnemies = false;
            foreach (var id in world.Enumerate(_killAllFilterId))
            {
                haveEnemies = true;
                break;
            }
            isGoalComplete = !haveEnemies;
        }
        //EGoal.CompleteLevel is handled by LevelExitCollsionSystem
        
        if (isGoalComplete)
        {
            MiscUtils.AddScore(Constants.PointsForGoalCompletion);
            SceneManager.LoadScene(Constants.MainMenu);
        }
    }
}