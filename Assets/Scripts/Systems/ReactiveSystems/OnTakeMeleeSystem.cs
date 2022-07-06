using ECS;
using Tags;

[ReactiveSystem(EReactionType.OnAdd, typeof(MeleeWeaponHoldingTag))]
public static class OnTakeMeleeSystem
{
    private static BitMask _includes1 = new BitMask(ComponentMeta<ProjectileWeaponHoldingTag>.Id);
    private static BitMask _includes2 = new BitMask(ComponentMeta<InstantRangedWeaponHoldingTag>.Id);

    public static void Tick(EcsWorld world, int id)
    {
        if (world.CheckAgainstMasks(id, _includes1))
            world.RemoveComponent<ProjectileWeaponHoldingTag>(id);
        if (world.CheckAgainstMasks(id, _includes2))
            world.RemoveComponent<InstantRangedWeaponHoldingTag>(id);
    }
}
