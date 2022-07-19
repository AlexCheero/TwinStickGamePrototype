using Components;
using ECS;
using Tags;
using UnityEngine;

/*
 * this system should go before any specific attack system and after any attack triggering system
 * e.g. PlayerAttackSystem, EnemyAttackSystem, ***AttackCDSystem***, MeleeAttackSYstem, RangeAttackSystem, ProjectileAttackSystem
 */

[System(ESystemCategory.Update)]
public class AttackCDSystem : EcsSystem
{
    private int _filterId;

    public AttackCDSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<Attack>(), Id<AttackCooldown>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            ref var attackCD = ref world.GetComponentByRef<AttackCooldown>(id);
            var nextAttackTime = attackCD.previousAttackTime + attackCD.attackCD;
            if (Time.time < nextAttackTime)
                world.Remove<Attack>(id);
            else
                attackCD.previousAttackTime = Time.time;
        }
    }
}