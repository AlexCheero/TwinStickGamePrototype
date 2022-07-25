using Components;
using ECS;
using Tags;

//choose system type here
[System(ESystemCategory.Update)]
public class FSMAISystem : EcsSystem
{
    private BitMask GeneralIncludes =
        new BitMask(
            Id<EnemyTag>(),
            Id<HealthComponent>(),
            Id<HealthLimitsComponent>(),
            Id<CurrentWeapon>()
        );

    private int _patrolFilterId;
    private int _chaseFilterId;
    private int _attackFilterId;
    private int _fleeFilterId;

    public FSMAISystem(EcsWorld world)
    {
        var patrolMask = GeneralIncludes;
        patrolMask.Set(Id<PatrolState>());
        _patrolFilterId = world.RegisterFilter(patrolMask);

        var chaseMask = GeneralIncludes;
        chaseMask.Set(Id<ChaseState>());
        _chaseFilterId = world.RegisterFilter(chaseMask);

        var attackMask = GeneralIncludes;
        attackMask.Set(Id<AttackState>());
        _attackFilterId = world.RegisterFilter(attackMask);

        var fleeMask = GeneralIncludes;
        fleeMask.Set(Id<FleeState>());
        _fleeFilterId = world.RegisterFilter(fleeMask);
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_patrolFilterId))
            Patrol(id, world);

        foreach (var id in world.Enumerate(_chaseFilterId))
            Chase(id, world);

        foreach (var id in world.Enumerate(_attackFilterId))
            Attack(id, world);

        foreach (var id in world.Enumerate(_fleeFilterId))
            Flee(id, world);
    }

    private void Patrol(int id, EcsWorld world)
    {
        /*
         * move through patrol route
         * if enemy noticed remove patrol state and set chase state
         */
    }

    private void Chase(int id, EcsWorld world)
    {
        /*
         * chase enemy
         * if distance is enough remove chase state and set attack state
         * if health is low remove chase state and set flee state
         */
    }

    private void Attack(int id, EcsWorld world)
    {
        /*
         * attack enemy
         * if distance is not enough remove attack state and set chase state
         * if health is low remove attack state and set flee state
         */
    }

    private void Flee(int id, EcsWorld world)
    {
        /*
         * run from enemy
         */
    }
}