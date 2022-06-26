using UnityEngine;

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

    struct TargetTransformComponent
    {
        public Transform target;
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

    struct DamageComponent
    {
        public float damage;
    }

    struct ProjectileWeapon
    {
        public ProjectileConvertible projectile;
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
    struct InstantRangedWeaponHoldingTag { }
    struct ProjectileWeaponHoldingTag { }
    struct MeleeWeaponHoldingTag { }
}