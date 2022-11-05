using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum EGoal
{
    KillAll,
    CompleteLevel
}

public class MapGoal : Singleton<MapGoal>
{
    [SerializeField]
    private EGoal _goal;
    
    public EGoal Goal { get { return _goal; } }

    protected override void Awake()
    {
        base.Awake();
        RandomizeGoal();
    }

    public void RandomizeGoal()
    {
        var goals = Enum.GetValues(typeof(EGoal));
        var goalIdx = Random.Range(0, goals.Length);
        _goal = (EGoal)goals.GetValue(goalIdx);
    }
}
