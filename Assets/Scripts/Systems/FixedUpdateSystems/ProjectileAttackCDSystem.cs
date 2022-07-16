using Components;
using ECS;
using Tags;
using UnityEngine;

//choose system type here
[System(ESystemCategory.FixedUpdate)]
public class ProjectileAttackCDSystem : EcsSystem
{
    private int _filterId;

    public ProjectileAttackCDSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<Attack>(), Id<AttackCooldown>(), Id<ProjectileWeapon>()));
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