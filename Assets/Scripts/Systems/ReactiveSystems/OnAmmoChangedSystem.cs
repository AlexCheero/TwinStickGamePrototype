using Components;
using Tags;
using ECS;

[ReactiveSystem(EReactionType.OnChange, typeof(Ammo))]
public static class OnAmmoChangedSystem
{
    public static void Tick(EcsWorld world, int id, Ammo oldVal, Ammo newVal)
    {
        if (newVal.amount <= 0)
        {
            world.RemoveComponent<Ammo>(id);
            world.Add<MeleeWeaponHoldingTag>(id);
        }
    }
}
