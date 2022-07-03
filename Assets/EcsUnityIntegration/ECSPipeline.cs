using ECS;
using System;
using System.Linq;
using UnityEngine;

public class ECSPipeline : MonoBehaviour
{
    private EcsWorld _world;
    
    private EcsSystem[] _initSystems;
    private EcsSystem[] _updateSystems;
    private EcsSystem[] _fixedUpdateSystems;

    //TODO: same as for EntityView: define different access modifiers for UNITY_EDITOR
    [SerializeField]
    public string[] _initSystemTypeNames;
    [SerializeField]
    public string[] _updateSystemTypeNames;
    [SerializeField]
    public string[] _fixedUpdateSystemTypeNames;

    void Start()
    {
        _world = new EcsWorld();

        //var systemCtorParams = new object[] { _world };
        //_initSystems = CreateSystemsByNames(_initSystemTypeNames, systemCtorParams);
        //_updateSystems = CreateSystemsByNames(_updateSystemTypeNames, systemCtorParams);
        //_fixedUpdateSystems = CreateSystemsByNames(_fixedUpdateSystemTypeNames, systemCtorParams);
        
        //all filters should be registered before adding any components
        //therefore all systems should be created before initing EntityViews
        _initSystems = new EcsSystem[]
        {
            new InitCameraSystem(_world),
            new InitResetAttackTimeSystem(_world),
            new InitEnemySystem(_world),
            new InitPlayerColliderSystem(_world)
        };

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
        foreach (var system in _initSystems)
            system.Tick(_world);
    }

    void Update()
    {
        foreach (var system in _updateSystems)
            system.Tick(_world);
    }

    private EcsSystem[] CreateSystemsByNames(string[] names, object[] systemCtorParams)
    {
        if (names.Length < 1)
            return null;

        var systems = new EcsSystem[names.Length];

        for (int i = 0; i < names.Length; i++)
        {
            var systemType = IntegrationHelper.GetTypeByName(names[i], EGatheredTypeCategory.System);
#if DEBUG
            if (systemType == null)
                throw new Exception("can't find system type");
#endif
            systems[i] = (EcsSystem)Activator.CreateInstance(systemType, systemCtorParams);
        }

        return systems;
    }

#if UNITY_EDITOR
    public bool AddSystem(string systemName, ESystemCategory systemCategory)
    {
        switch (systemCategory)
        {
            case ESystemCategory.Init:
                return AddSystem(systemName, ref _initSystemTypeNames);
            case ESystemCategory.Update:
                return AddSystem(systemName, ref _updateSystemTypeNames);
            case ESystemCategory.FixedUpdate:
                return AddSystem(systemName, ref _fixedUpdateSystemTypeNames);
            default:
                return false;
        }
    }

    private bool AddSystem(string systemName, ref string[] systems)
    {
        if (systems == null)
            systems = new string[0];

        foreach (var sysName in systems)
            if (systemName == sysName) return false;

        Array.Resize(ref systems, systems.Length + 1);
        systems[systems.Length - 1] = systemName;

        return true;
    }
#endif
}
