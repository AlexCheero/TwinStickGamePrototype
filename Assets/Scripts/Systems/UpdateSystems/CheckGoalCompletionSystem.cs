using System;
using ECS;
using Tags;
using UnityEngine.SceneManagement;

//choose system type here
[System(ESystemCategory.Update)]
public class CheckGoalCompletionSystem : EcsSystem
{
    private int _killAllFilterId;

    public CheckGoalCompletionSystem(EcsWorld world)
    {
        _killAllFilterId = world.RegisterFilter(new BitMask(Id<EnemyTag>()), new BitMask(Id<DeadTag>()));
    }

    public override void Tick(EcsWorld world)
    {
        var isGoalComplete = false;
        var goal = MapGoal.Instance.Goal;
        if (goal == EGoal.KillAll)
        {
            //TODO: implement count property for filters
            int enemyCount = 0;
            foreach (var id in world.Enumerate(_killAllFilterId))
                enemyCount++;
            isGoalComplete = enemyCount == 0;
        }
        //EGoal.CompleteLevel is handled by LevelExitCollsionSystem
        
        if (isGoalComplete)
        {
            MiscUtils.AddScore(Constants.PointsForGoalCompletion);
            SceneManager.LoadScene(Constants.MainMenu);
        }
    }
}