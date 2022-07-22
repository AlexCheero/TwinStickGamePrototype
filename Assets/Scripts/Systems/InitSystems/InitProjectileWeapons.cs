using Components;
using ECS;

//choose system type here
[System(ESystemCategory.Init)]
public class InitProjectileWeapons : EcsSystem
{
    private int _filterId;

    public InitProjectileWeapons(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<ProjectileWeapon>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            ref var projectileWeapon = ref world.GetComponentByRef<ProjectileWeapon>(id);
            projectileWeapon.poolName = "ProjectilePool";
            projectileWeapon.prototypeEntity = PoolManager.Get(projectileWeapon.poolName).GetPrototype<EntityView>().Entity;
        }
    }
}