using Components;
using Tags;
using ECS;
using UnityEngine;

[ReactiveSystem(EReactionType.OnChange, typeof(Ammo))]
public static class OnAmmoChangedSystem
{
    public static void Tick(EcsWorld world, int id, Ammo oldVal, Ammo newVal)
    {
#if DEBUG
        if (oldVal.amount == newVal.amount)
            throw new System.Exception("ammo amount didn't changed");
#endif
        Debug.Log("OnAmmoChangedSystem.Tick. ammo left: " + newVal.amount);
        if (newVal.amount <= 0)
        {
            world.RemoveComponent<Ammo>(id);
            world.Add<MeleeWeaponHoldingTag>(id);
        }
    }
}
