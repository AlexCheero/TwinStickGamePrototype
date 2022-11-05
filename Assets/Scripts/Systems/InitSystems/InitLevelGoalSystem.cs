using System;
using Components;
using ECS;
using UnityEngine;
using Random = UnityEngine.Random;

//choose system type here
[System(ESystemCategory.Init)]
public class InitLevelGoalSystem : EcsSystem
{
#if DEBUG
    private readonly int _filterId;
#endif

    public InitLevelGoalSystem(EcsWorld world)
    {
#if DEBUG
        _filterId = world.RegisterFilter(new BitMask(Id<LevelGoal>()));
#endif
    }

    public override void Tick(EcsWorld world)
    {
#if DEBUG
        foreach (var id in world.Enumerate(_filterId))
            throw new Exception("goal already inited");
#endif

        var goals = Enum.GetValues(typeof(EGoal));
        var goalIdx = Random.Range(0, goals.Length);
        var goal = (EGoal)goals.GetValue(goalIdx);
        
        Debug.Log("Goal: " + goal);
        
        var goalId = world.Create();
        world.Add(goalId, new LevelGoal { goal = goal });
    }
}