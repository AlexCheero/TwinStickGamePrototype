using ECS;
using UnityEngine;

public class ECSPipeline : MonoBehaviour
{
    private EcsWorld _world;
    private EcsSystem[] _updateSystems;

    void Start()
    {
        _world = new EcsWorld();

        //all filters should be registered before adding any components
        //therefore all systems should be created before initing EntityViews
        EcsSystem[] initSystems = new EcsSystem[]
        {
            new InitCameraSystem(_world),
            new InitResetAttackTimeSystem(_world),
            new InitEnemySystem(_world)
        };

        //TODO: implement custom inspector to arrange systems from editor
        //TODO: implement script to generate boilerplate
        _updateSystems = new EcsSystem[]
        {
            new CleanupTargetEntitySystem(_world),
            new PlayerMovementSystem(_world),
            new CameraFollowSystem(_world),
            new PlayerRotationSystem(_world),
            new EnemyFollowSystem(_world),
            new HealthSystem(_world),
            new EnemyMeleeAttackSystem(_world),
            new PlayerMeleeAttackSystem(_world),
            new PlayerInstantRangedAttackSystem(_world),
            new PlayerProjectileAttackSystem(_world)
        };

        foreach (var view in FindObjectsOfType<EntityView>())
            view.InitAsEntity(_world);

        //call init systems after initing all the start entities
        foreach (var system in initSystems)
            system.Tick(_world);
    }

    void Update()
    {
        foreach (var system in _updateSystems)
            system.Tick(_world);
    }
}
