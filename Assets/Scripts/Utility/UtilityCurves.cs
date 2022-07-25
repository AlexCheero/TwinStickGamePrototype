using UnityEngine;

[CreateAssetMenu(fileName = "UtilityCurves", menuName = "UtilityAI/New utility curves", order = -1)]
public class UtilityCurves : ScriptableObject
{
    public AnimationCurve Health;
    public AnimationCurve Damage;
    public AnimationCurve DistanceToTarget;
}
