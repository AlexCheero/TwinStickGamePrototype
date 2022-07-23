using Components;
using ECS;
using Tags;
using UnityEngine;

//choose system type here
[System(ESystemCategory.Update)]
public class UtilityBasedDecisionMakingSystem : EcsSystem
{
    private int _filterId;

    public UtilityBasedDecisionMakingSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(
            new BitMask(
                Id<EnemyTag>(),
                Id<HealthComponent>(),
                Id<HealthLimitsComponent>(),
                Id<UtilityCurvesComponent>(),
                Id<CurrentWeapon>())
            );
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            var normalizedHealth = GetNormalizedHealth(world, id);

            var targetHealth = GetTargetHealth(world, id);
            var normalizedDamage = Mathf.Clamp01(GetWeaponDamage(world, id) / targetHealth);

            var curves = world.GetComponent<UtilityCurvesComponent>(id);
            var getHealthUtility = Evaluate(curves.health, normalizedHealth);
            var targetUtility = targetHealth > 0 ? 1 : 0;
            var attackUtility = Evaluate(curves.damage, normalizedDamage);

            var patrolPriority = 1 - targetUtility;
            var attackPriority = ((1 - getHealthUtility) + targetUtility + attackUtility) / 3;
            var fleePriority = (getHealthUtility + targetUtility) / 2;

            if (attackPriority > fleePriority && attackPriority > patrolPriority)
                world.Add<ChaseAction>(id);
            else if (fleePriority > patrolPriority && fleePriority > attackPriority)
                world.Add<FleeAction>(id);
            else
                world.Add<PatrolAction>(id);
        }
    }

    private float GetNormalizedHealth(EcsWorld world, int id)
    {
        var health = world.GetComponent<HealthComponent>(id).health;
        var maxHealth = world.GetComponent<HealthLimitsComponent>(id).maxHealth;
#if DEBUG
        if (health > maxHealth)
            throw new System.Exception("health can't be bigger than max health");
#endif
        return health / maxHealth;
    }

    private float GetTargetHealth(EcsWorld world, int id)
    {
        if (world.Have<TargetEntityComponent>(id))
        {
            var targetEntity = world.GetComponent<TargetEntityComponent>(id).target;
#if DEBUG
            if (targetEntity == null)
                throw new System.Exception("TargetEntityComponent should be cleaned up if target entity view is null");
#endif
            
            if (world.IsEntityValid(targetEntity.Entity) &&
                world.Have<HealthComponent>(targetEntity.Id))
            {
                return world.GetComponent<HealthComponent>(targetEntity.Id).health;
            }
        }

        return 0;
    }

    private float GetWeaponDamage(EcsWorld world, int id)
    {
        var weaponEntity = world.GetComponent<CurrentWeapon>(id).entity;
        var weaponId = weaponEntity.GetId();
#if DEBUG
        if (!world.Have<ProjectileWeapon>(weaponId) && !world.Have<DamageComponent>(weaponId))
            throw new System.Exception("weapon should have damage component");
        if (!world.Have<MeleeWeapon>(weaponId) && !world.Have<Ammo>(weaponId))
            throw new System.Exception("only melee weapon can have no ammo component");
#endif

        if (world.Have<Ammo>(weaponId) && world.GetComponent<Ammo>(id).amount <= 0)
            return 0;

        float damage = 0f;
        if (world.Have<ProjectileWeapon>(weaponId))
        {
            var projectileEntity = world.GetComponent<ProjectileWeapon>(weaponId).prototypeEntity;
            var projectileId = projectileEntity.GetId();
#if DEBUG
            if (!world.IsEntityValid(projectileEntity))
                throw new System.Exception("invalid prototype projectile entity");
            if (!world.Have<DamageComponent>(projectileId))
                throw new System.Exception("projectile should have damage component");
#endif
            damage = world.GetComponent<DamageComponent>(projectileId).damage;
        }
        else
        {
            damage = world.GetComponent<DamageComponent>(weaponId).damage;
        }

        return damage;
    }

    private float Evaluate(in UtilityCurve curves, float value)
    {
        return 0;
    }
}