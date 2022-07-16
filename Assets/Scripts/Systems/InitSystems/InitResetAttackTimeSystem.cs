using Components;
using ECS;

[System(ESystemCategory.Init)]
public class InitResetAttackTimeSystem : EcsSystem
{
    private int _filterId;

    public InitResetAttackTimeSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<AttackCooldown>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
            world.GetComponentByRef<AttackCooldown>(id).previousAttackTime = -1;
    }
}
