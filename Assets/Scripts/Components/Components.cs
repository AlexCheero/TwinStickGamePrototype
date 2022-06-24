#region Components
using UnityEngine;

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

struct MeleeAttackReachComponent
{
    //it takes in account the radius of player and enemy
    public float distance;
}

struct MeleeDamageComponent
{
    public float damage;
}

struct MeleeAttackComponent
{
    public float previousAttackTime;
    public float attackCD;
}
#endregion

#region Tags
struct PlayerTag { }
struct EnemyTag { }
struct CameraTag { }
#endregion