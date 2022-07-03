using ECS;
using System;
using UnityEngine;

public class ECSPipeline : MonoBehaviour
{
    private EcsWorld _world;
    
    private EcsSystem[] _initSystems;
    private EcsSystem[] _updateSystems;
    private EcsSystem[] _fixedUpdateSystems;

    //TODO: same as for EntityView: define different access modifiers for UNITY_EDITOR
    [SerializeField]
    public string[] _initSystemTypeNames = new string[0];
    [SerializeField]
    public string[] _updateSystemTypeNames = new string[0];
    [SerializeField]
    public string[] _fixedUpdateSystemTypeNames = new string[0];

#if UNITY_EDITOR
    [SerializeField]
    public bool[] _initSwitches = new bool[0];
    [SerializeField]
    public bool[] _updateSwitches = new bool[0];
    [SerializeField]
    public bool[] _fixedUpdateSwitches = new bool[0];
#endif

    void Start()
    {
        _world = new EcsWorld();

        var systemCtorParams = new object[] { _world };
        _initSystems = CreateSystemsByNames(_initSystemTypeNames, systemCtorParams);
        _updateSystems = CreateSystemsByNames(_updateSystemTypeNames, systemCtorParams);
        _fixedUpdateSystems = CreateSystemsByNames(_fixedUpdateSystemTypeNames, systemCtorParams);
        
        /*
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
        */

        foreach (var view in FindObjectsOfType<EntityView>())
            view.InitAsEntity(_world);

        //call init systems after initing all the start entities
#if UNITY_EDITOR
        TickSystemCategory(_initSystems, _initSwitches);
#else
        TickSystemCategory(_initSystems);
#endif
    }

    void Update()
    {
#if UNITY_EDITOR
        TickSystemCategory(_updateSystems, _updateSwitches);
#else
        TickSystemCategory(_updateSystems);
#endif
    }

#if UNITY_EDITOR
    private void TickSystemCategory(EcsSystem[] systems, bool[] switches)
#else
    private void TickSystemCategory(EcsSystem[] systems)
#endif
    {
        for (int i = 0; i < systems.Length; i++)
        {
#if UNITY_EDITOR
            if (switches[i])
#endif
                systems[i].Tick(_world);
        }
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
                return AddSystem(systemName, ref _initSystemTypeNames, ref _initSwitches);
            case ESystemCategory.Update:
                return AddSystem(systemName, ref _updateSystemTypeNames, ref _updateSwitches);
            case ESystemCategory.FixedUpdate:
                return AddSystem(systemName, ref _fixedUpdateSystemTypeNames, ref _fixedUpdateSwitches);
            default:
                return false;
        }
    }

    private bool AddSystem(string systemName, ref string[] systems, ref bool[] switches)
    {
        foreach (var sysName in systems)
            if (systemName == sysName) return false;

        Array.Resize(ref systems, systems.Length + 1);
        systems[systems.Length - 1] = systemName;

        Array.Resize(ref switches, switches.Length + 1);
        switches[switches.Length - 1] = true; ;

        return true;
    }

    public void RemoveMetaAt(ESystemCategory systemCategory, int idx)
    {
        switch (systemCategory)
        {
            case ESystemCategory.Init:
                RemoveMetaAt(idx, ref _initSystemTypeNames, ref _initSwitches);
                break;
            case ESystemCategory.Update:
                RemoveMetaAt(idx, ref _updateSystemTypeNames, ref _updateSwitches);
                break;
            case ESystemCategory.FixedUpdate:
                RemoveMetaAt(idx, ref _fixedUpdateSystemTypeNames, ref _fixedUpdateSwitches);
                break;
        }
    }

    private void RemoveMetaAt(int idx, ref string[] systems, ref bool[] switches)
    {
        var newLength = systems.Length - 1;
        for (int i = idx; i < newLength; i++)
        {
            systems[i] = systems[i + 1];
            switches[i] = switches[i + 1];
        }
        Array.Resize(ref systems, newLength);
    }
#endif
}
