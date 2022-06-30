using Components;
using ECS;

public class InitResetAttackTimeSystem : EcsSystem
{
    private int _filterId;

    public InitResetAttackTimeSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<AttackComponent>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
            world.GetComponent<AttackComponent>(id).previousAttackTime = -1;
    }
}
