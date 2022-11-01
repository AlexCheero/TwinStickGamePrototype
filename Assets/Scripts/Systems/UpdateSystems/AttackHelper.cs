using System;
using Components;
using ECS;
using UnityEngine;

public static class AttackHelper
{
    public static bool CheckAndUpdateAttackCooldown(EcsWorld world, int id)
    {
        ref var attackCD = ref world.GetComponentByRef<AttackCooldown>(id);
        var nextAttackTime = attackCD.previousAttackTime + attackCD.attackCD;
        var isCooldownExpired = Time.time >= nextAttackTime;
        if (isCooldownExpired)
            attackCD.previousAttackTime = Time.time;
        return isCooldownExpired;
    }
    
    public static bool PlayAttackAnimationState(Animator animator, float playTime, string triggerName)
    {
        animator.SetTrigger(triggerName);
                
        //prepare attack animation speed
        animator.Update(0);//hack to be able to get next animator state
        //if no transition nextStateInfo.length will be 0 and animator will stuck
        if (!animator.IsInTransition(1))
            return false;
        var nextStateInfo = animator.GetNextAnimatorStateInfo(1);
        var attackTime = nextStateInfo.length * nextStateInfo.speedMultiplier;
        var speedMultiplier = attackTime / playTime;
#if DEBUG
        if (speedMultiplier <= float.Epsilon)
            throw new Exception("speedMultiplier should be bigger than 0");
#endif
        animator.SetFloat("AttackSpeedMultiplier", speedMultiplier);

        return true;
    }
}