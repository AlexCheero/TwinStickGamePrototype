using Components;
using ECS;
using Tags;
using UnityEngine;
using UnityEngine.SceneManagement;

//choose system type here
[System(ESystemCategory.LateFixedUpdate)]
public class LevelExitCollisionSystem : EcsSystem
{
    private readonly int _goalFilterId;
    private readonly int _filterId;
    private readonly int _levelSettingsFilterId;

    public LevelExitCollisionSystem(EcsWorld world)
    {
        _goalFilterId = world.RegisterFilter(new BitMask(Id<LevelGoal>()));
        _filterId = world.RegisterFilter(new BitMask(Id<LevelExit>(), Id<CollisionWith>()));
        _levelSettingsFilterId = world.RegisterFilter(new BitMask(Id<LevelSettingsComponent>()));
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
            {
                Debug.Log("something collided level exit");
                continue;
            }

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
        stashHolder.CurrentWeaponType = GetCurrentWeaponType(world, playerId);
    }

    private void StashWeapon(EcsWorld world, EWeaponType type, Entity entity)
    {
        if (!world.IsEntityValid(entity))
            return;
        var stashHolder = EntityStashHolder.Instance;
        stashHolder.Weapons[type] = world.GetComponent<Prototype>(entity.GetId()).prefab;
    }

    private EWeaponType GetCurrentWeaponType(EcsWorld world, int playerId)
    {
        var currentWeaponId = world.GetComponent<CurrentWeapon>(playerId).entity.GetId();

        if (world.Have<RangedWeapon>(currentWeaponId))
            return EWeaponType.Ranged;
        if (world.Have<ProjectileWeapon>(currentWeaponId))
            return EWeaponType.Projectile;
        return EWeaponType.Melee;
    }
}