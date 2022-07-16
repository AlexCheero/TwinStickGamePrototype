using Components;
using ECS;

[System(ESystemCategory.Update)]
public class AmmoCountSystem : EcsSystem
{
    private int _filterId;

    public AmmoCountSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<Ammo>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            if (world.GetComponent<Ammo>(id).amount <= 0)
                world.Remove<Ammo>(id);
        }
    }
}