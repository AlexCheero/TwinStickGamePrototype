using System;
using Components;
using ECS;
using Tags;
using UnityEngine.SceneManagement;

//choose system type here
[System(ESystemCategory.LateFixedUpdate)]
public class LevelExitCollisionSystem : EcsSystem
{
    private readonly int _goalFilterId;
    private readonly int _filterId;

    public LevelExitCollisionSystem(EcsWorld world)
    {
        _goalFilterId = world.RegisterFilter(new BitMask(Id<LevelGoal>()));
        _filterId = world.RegisterFilter(new BitMask(Id<LevelExit>(), Id<CollisionWith>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_goalFilterId))
        {
            if (world.GetComponent<LevelGoal>(id).goal != EGoal.CompleteLevel)
                return;
        }

        foreach (var id in world.Enumerate(_filterId))
        {
            var collidedId = world.GetComponent<CollisionWith>(id).entity.GetId();
            if (!world.Have<PlayerTag>(collidedId))
                continue;

            SaveStash(world, collidedId);
            
            MiscUtils.AddScore(Constants.PointsForGoalCompletion);
            SceneManager.LoadScene(Constants.ProceduralLevel);
            break;
        }
    }

    private void SaveStash(EcsWorld world, int playerId)
    {
        var stashHolder = EntityStashHolder.Instance;
        stashHolder.Health = world.GetComponent<HealthComponent>(playerId).health;
        
        var weaponry = world.GetComponent<Weaponry>(playerId);
        StashWeapon(world, EWeaponType.Melee, weaponry.melee);
        StashWeapon(world, EWeaponType.Ranged, weaponry.ranged);
        StashWeapon(world, EWeaponType.Projectile, weaponry.throwable);
        
        var currentWeaponId = world.GetComponent<CurrentWeapon>(playerId).entity.GetId();
        stashHolder.CurrentWeaponType = WeaponHelper.GetWeaponType(world, currentWeaponId);
    }

    private void StashWeapon(EcsWorld world, EWeaponType type, Entity entity)
    {
        if (!world.IsEntityValid(entity))
            return;
        var id = entity.GetId();
        if (!world.Have<Prototype>(id))
            return;
#if DEBUG
        var actualType = WeaponHelper.GetWeaponType(world, id);
        if (type != actualType)
            throw new Exception("Weapon type mismatch!");
#endif
        
        var stashHolder = EntityStashHolder.Instance;
        var weaponStash = new WeaponStashData
        {
            Prefab = world.GetComponent<Prototype>(id).prefab,
            Ammo = world.Have<Ammo>(id) ? world.GetComponent<Ammo>(id).amount : 0
        };
        stashHolder.WeaponPrefabs[type] = weaponStash;
    }
}