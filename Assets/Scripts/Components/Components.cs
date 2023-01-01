using ECS;
using UnityEngine;
using UnityEngine.UI;

public enum EAmmoType
{
    Shell,
    Grenade
}

public enum EGoal
{
    KillAll,
    CompleteLevel
}

//TODO: remove prefixes
namespace Components
{
    struct SpeedComponent
    {
        public float speed;
    }

    struct AngularSpeedComponent
    {
        public float speed;
    }

    struct AccelerationComponent
    {
        public float acceleration;
    }

    struct TargetEntityComponent
    {
        public EntityView target;
    }

    struct CameraSettingsComponent
    {
        public Vector3 direction;
        public float distance;
    }

    struct ImpactEffect
    {
        public string poolName;
    }
    
    struct HealthComponent
    {
        public float health;
    }

    struct HealthUISlider
    {
        public Slider slider;
    }

    struct WeaponUI
    {
        public Image image;
        public Text text;
    }

    struct WeaponIcon
    {
        public Sprite icon;
    }

    struct HealthLimitsComponent
    {
        public float maxHealth;
    }

    struct ReachComponent
    {
        //it takes in account the radius of player and enemy
        public float distance;
    }

    struct AttackReachComponent
    {
        //it takes in account the radius of player and enemy
        public float distance;
    }

    struct ViewAngle
    {
        public float angle;
    }

    struct ViewDistance
    {
        public float distance;
    }

    struct AttackAngle
    {
        public float angle;
    }

    struct DamageComponent
    {
        public float damage;
    }

    struct Impact
    {
        public Vector3 position;
        public Vector3 normal;
    }

    struct AttackCooldown
    {
        [HiddenInspector]
        [DefaultValue(-1)]
        public float previousAttackTime;
        public float attackCD;
    }

    struct LifeTime
    {
        public float time;
    }

    public struct Ammo
    {
        public int amount;
        public EAmmoType type;
    }

    public struct CurrentWeapon
    {
        public Entity entity;
    }

    struct LevelGoal
    {
        public EGoal goal;
    }

    struct AttackEvent
    {
        public Vector3 position;
        public Vector3 direction;
    }

    struct GrenadeFlyEvent
    {
        public Vector3 position;
        public Vector3 direction;
    }

    struct MeleeAttackEvent
    {
        public Vector3 position;
        public Vector3 direction;
    }

    struct DefaultWeapon
    {
        public EntityPreset preset;
    }

    struct PlayerVelocityComponent
    {
        public Vector3 velocity;
    }

    struct PlayerDirectionComponent
    {
        public Vector3 direction;
    }

    struct ProjectileWeapon
    {
        public string poolName;
    }

    struct Owner
    {
        public Entity entity;
    }

    struct NextWaypointIdx
    {
        public int idx;
    }

    struct GripTransform
    {
        public Vector3 position;
        public Vector3 rotation;
    }

    public struct Weaponry
    {
        [HiddenInspector]
        public Entity melee;
        [HiddenInspector]
        public Entity ranged;
        [HiddenInspector]
        public Entity throwable;
    }

    /*
    struct UtilityCurve
    {
        public float h0;
        public float h1;
        public float h2;
        public float h3;
        public float h4;
        public float h5;
        public float h6;
        public float h7;
        public float h8;
        public float h9;
        public float h10;
    }

    struct UtilityCurvesComponent
    {
        public UtilityCurve health;
        public UtilityCurve damage;
    }
    */

    struct UtilityCurvesComponent
    {
        public UtilityCurves curves;
    }

    struct LevelSettingsComponent
    {
        public LevelSettings Settings;
    }

    struct RandomCountRange
    {
        public int min;
        public int max;
    }

    struct Prototype
    {
        public EntityView prefab;
    }

    struct PlayerSight
    {
        public Vector3 Start;
        public Vector3 End;
        public Vector3 Normal;
        public EntityView SightedView;
    }

    struct ViewOffset
    {
        public Vector3 offset;
    }
}

namespace Tags
{
    struct CharacterTag { }
    struct PlayerTag { }
    struct EnemyTag { }
    struct CameraTag { }
    struct Projectile { }
    struct Pickup { }
    struct DeleteOnCollision { }
    struct DeadTag { }
    struct MeleeWeapon { }
    struct DefaultMeleeWeapon { }
    struct RangedWeapon { }
    struct Weapon { }
    struct SeenEnemyTag { }
    struct LevelExit { }

    //stuff needed for more complex AI, which was abandoned
    struct PatrolAction { }
    struct ChaseAction { }
    struct AttackAction { }
    struct FleeAction { }

    struct PatrolState { }
    struct ChaseState { }
    struct AttackState { }
    struct FleeState { }
}