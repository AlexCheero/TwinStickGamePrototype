using Components;
using ECS;
using System;
using System.Collections.Generic;
using Tags;
using UnityEngine;

//choose system type here
[System(ESystemCategory.Update)]
public class UtilityBasedDecisionMakingSystem : EcsSystem
{
    private enum EActions
    {
        Patrol,
        Chase,
        Attack,
        Flee
    }

    private struct ActionPriority
    {
        public EActions Type;
        public float Priority;

        public ActionPriority(EActions type, float priority)
        {
            Type = type;
            Priority = priority;
        }
    }

    private class PriorityComparer : IComparer<ActionPriority>
    {
        //sort in descending order
        public int Compare(ActionPriority x, ActionPriority y)
        {
            if (x.Priority < y.Priority)
                return 1;
            if (y.Priority < x.Priority)
                return -1;
            return 0;
        }
    }

    private int _filterId;
    private ActionPriority[] _priorities;
    private PriorityComparer _comparer;

    public UtilityBasedDecisionMakingSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(
            new BitMask(
                Id<EnemyTag>(),
                Id<HealthComponent>(),
                Id<HealthLimitsComponent>(),
                Id<UtilityCurvesComponent>(),
                Id<AttackReachComponent>(),
                Id<Transform>(),
                Id<CurrentWeapon>())
            );

        _priorities = new ActionPriority[4] { default, default, default, default, };
        _comparer = new PriorityComparer();
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            var normalizedHealth = GetNormalizedHealth(world, id);

            EntityView targetView = null;
            if (world.Have<TargetEntityComponent>(id))
            {
                targetView = world.GetComponent<TargetEntityComponent>(id).target;
#if DEBUG
                if (targetView == null)
                    throw new Exception("TargetEntityComponent should be cleaned up if target entity view is null");
                if (!world.IsEntityValid(targetView.Entity))
                    throw new Exception("TargetEntityComponent should be cleaned up if target entity view is invalid");
#endif
            }

            var targetHealth = targetView != null && world.Have<HealthComponent>(targetView.Id)
                ? world.GetComponent<HealthComponent>(targetView.Id).health : 0;
            var normalizedDamage = targetHealth > 0 ? Mathf.Clamp01(GetWeaponDamage(world, id) / targetHealth) : 1;

            var position = world.GetComponent<Transform>(id).position;
            var distance = (targetView.transform.position - position).magnitude;
            var normalizedDistance = Mathf.Clamp01(distance / world.GetComponent<AttackReachComponent>(id).distance);

            var curves = world.GetComponent<UtilityCurvesComponent>(id).curves;
            var healthUtility = curves.Health.Evaluate(normalizedHealth);
            var targetUtility = targetHealth > 0 ? 1 : 0;
            var attackUtility = curves.Damage.Evaluate(normalizedDamage);
            var distanceUtility = curves.DistanceToTarget.Evaluate(normalizedDistance);

            var patrolPriority = 1 - targetUtility;
            _priorities[0] = new ActionPriority(EActions.Patrol, patrolPriority);
            var chasePriority = ((1 - healthUtility) + targetUtility + attackUtility + distanceUtility) / 4;
            _priorities[1] = new ActionPriority(EActions.Chase, chasePriority);
            var attackPriority = ((1 - healthUtility) + targetUtility + attackUtility + (1 - distanceUtility)) / 4;
            _priorities[2] = new ActionPriority(EActions.Attack, attackPriority);
            var fleePriority = (healthUtility + targetUtility) / 2;
            _priorities[3] = new ActionPriority(EActions.Flee, fleePriority);

            Array.Sort(_priorities, _comparer);
            var highestPriorityAction = _priorities[0].Type;
            switch (highestPriorityAction)
            {
                case EActions.Patrol:
                    SetAction<PatrolAction>(world, id);
                    break;
                case EActions.Chase:
                    SetAction<ChaseAction>(world, id);
                    break;
                case EActions.Attack:
                    SetAction<AttackAction>(world, id);
                    break;
                case EActions.Flee:
                    SetAction<FleeAction>(world, id);
                    break;
            }
        }
    }

    private float GetNormalizedHealth(EcsWorld world, int id)
    {
        var health = world.GetComponent<HealthComponent>(id).health;
        var maxHealth = world.GetComponent<HealthLimitsComponent>(id).maxHealth;
#if DEBUG
        if (health > maxHealth)
            throw new Exception("health can't be bigger than max health");
#endif
        return health / maxHealth;
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
            throw new NotImplementedException("prototypeEntity is now not inited");
            var projectileEntity = EntityExtension.NullEntity;// world.GetComponent<ProjectileWeapon>(weaponId).prototypeEntity;
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

    private void SetAction<T>(EcsWorld world, int id)
    {
        if (world.Have<T>(id))
            return;

        Debug.Log("UtilityBasedDecisionMakingSystem.SetAction " + typeof(T).Name);
        //TODO: use mutually exclusive components here
        if (world.Have<PatrolAction>(id)) world.Remove<PatrolAction>(id);
        if (world.Have<ChaseAction>(id)) world.Remove<ChaseAction>(id);
        if (world.Have<AttackAction>(id)) world.Remove<AttackAction>(id);
        if (world.Have<FleeAction>(id)) world.Remove<FleeAction>(id);

        world.Add<T>(id);
    }
}