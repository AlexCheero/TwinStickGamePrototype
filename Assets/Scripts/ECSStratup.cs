using ECS;
using UnityEngine;

public class ECSStratup : MonoBehaviour
{
    private EcsWorld _world;
    private EcsSystem[] _updateSystems;

    void Start()
    {
        _world = new EcsWorld();
        _updateSystems = new EcsSystem[]
        {
            new PlayerMovementSystem(_world),
            new CameraFollowSystem(_world),
            new PlayerRotationSystem(_world),
            //new EnemyFollowSystem(_world),
            new HealthSystem(_world),
            new EnemyMeleeAttackSystem(_world),
            new PlayerMeleeAttackSystem(_world),
            new PlayerInstantRangedAttackSystem(_world),
            new PlayerProjectileAttackSystem(_world)
        };

        //TODO: implement EntityView with custom inspector and use it instead of ECSConvertible
        foreach (var convertible in FindObjectsOfType<ECSConvertible>())
            convertible.ConvertToEntity(_world);

        //Register and Run init systems if needed

        foreach (var view in FindObjectsOfType<EntityView>())
            view.InitAsEntity(_world);
    }

    void Update()
    {
        foreach (var system in _updateSystems)
            system.Tick(_world);
    }
}
