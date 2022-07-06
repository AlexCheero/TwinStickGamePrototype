using UnityEngine;

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

    struct HealthComponent
    {
        public float health;
    }

    struct ReachComponent
    {
        //it takes in account the radius of player and enemy
        public float distance;
    }

    struct ViewAngle
    {
        public float angle;
    }

    struct DamageComponent
    {
        public float damage;
    }

    struct ProjectileWeapon
    {
        public EntityView projectile;
    }

    struct AttackComponent
    {
        public float previousAttackTime;
        public float attackCD;
    }
}

namespace Tags
{
    struct PlayerTag { }
    struct EnemyTag { }
    struct CameraTag { }
    struct Projectile { }
    struct Pickup { }
    struct InstantRangedWeaponHoldingTag { }
    struct ProjectileWeaponHoldingTag { }
    struct MeleeWeaponHoldingTag { }
    struct DeleteOnCollision { }
    struct DeadTag { }
}