using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}
