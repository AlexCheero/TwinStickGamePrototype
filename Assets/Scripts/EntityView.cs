using ECS;
using System;
using System.Reflection;
using UnityEngine;

public enum EFieldType
{
    Int,
    Float,
    Vec3,
    //TODO: maybe could just load EntityView, instead of generic components
    SceneGO,//if go.scene.IsValid() then make a hierarchy by traversing all parents
    Prefab//else find the prefab path
}

[Serializable]
public struct ComponentFieldMeta
{
    //TODO: refine different access modifiers for UNITY_EDITOR
    public EFieldType Type;
    public string Name;
    public string _valueRepresentation;

    public object GetValue()
    {
        if (_valueRepresentation == null || _valueRepresentation.Length == 0)
            return null;

        switch (Type)
        {
            case EFieldType.Int:
                return int.Parse(_valueRepresentation);
            case EFieldType.Float:
                return float.Parse(_valueRepresentation);
            case EFieldType.Vec3:
                return ParseVector3(_valueRepresentation);
            case EFieldType.SceneGO:
                return ParseSceneGO(_valueRepresentation);
            case EFieldType.Prefab:
                return ParsePrefab(_valueRepresentation);
            default:
                return null;
        }
    }

    private Component ParsePrefab(string representation)
    {
        //loaded resource should be in Resources folder
        var typeNameAndPath = representation.Split(' ');
        var typeName = typeNameAndPath[0];
        var path = typeNameAndPath[1];
        var go = Resources.Load<GameObject>(path);
        return go.GetComponent(typeName);
    }

    private Component ParseSceneGO(string representation)
    {
        var typeNameAndHierarchy = representation.Split(' ');
        var typeName = typeNameAndHierarchy[0];
        var sceneHierarchy = typeNameAndHierarchy[1].Split('/');
        GameObject go = null;
        foreach (var gameObj in GameObject.FindObjectsOfType<GameObject>())
        {
            if (gameObj.name == sceneHierarchy[0] && gameObj.transform.parent == null)
            {
                go = gameObj;
                break;
            }
        }
        if (go == null)
        {
            Debug.LogError("can't find object hierarchy root");
            return null;
        }

        int hierarchyIdx = 1;
        while (hierarchyIdx < sceneHierarchy.Length)
        {
            bool isFound = false;
            foreach (Transform childTransform in go.transform)
            {
                if (childTransform.gameObject.name == sceneHierarchy[hierarchyIdx])
                {
                    go = childTransform.gameObject;
                    hierarchyIdx++;
                    isFound = true;
                    break;
                }
            }

            if (!isFound)
            {
                Debug.LogError("can't find object in hierarchy by name");
                return null;
            }
        }
        return go.GetComponent(typeName);
    }

    private Vector3 ParseVector3(string representation)
    {
        var representations = representation.Split(' ');
        if (representations.Length != 3)
        {
            Debug.LogError("wrong number of parameters to init vector3 from string");
            return Vector3.zero;
        }
        var x = float.Parse(representations[0]);
        var y = float.Parse(representations[1]);
        var z = float.Parse(representations[2]);
        return new Vector3(x, y, z);
    }
}

[Serializable]
public struct ComponentMeta
{
    public string ComponentName;
    public ComponentFieldMeta[] Fields;
#if UNITY_EDITOR
    public bool IsExpanded;
#endif
}

public class EntityView : MonoBehaviour
{
    private Entity _entity;
    private EcsWorld _world;

    private string _assemblyName;

    void Awake()
    {
        _assemblyName = typeof(EntityView).Assembly.FullName;
    }

    [SerializeField]
    private ComponentMeta[] _metas;

    public int MetasLength { get => _metas.Length; }
    public ref ComponentMeta GetMeta(int i) => ref _metas[i];

    public void InitAsEntity(EcsWorld world)
    {
        _world = world;

        var entityId = _world.Create();
        _entity = _world.GetById(entityId);

        MethodInfo addComponentInfo = typeof(EcsWorld).GetMethod("AddComponentNoReturn");

        //var meta = new ComponentMeta();
        //meta.ComponentName = "SpeedComponent";
        foreach (var meta in _metas)
        {
            var compType = Type.GetType(meta.ComponentName);
            MethodInfo addComponentInfoGen = addComponentInfo.MakeGenericMethod(compType);

            //TODO: try to remove postfix from component and tag types and spawn handle with specified namespace
            var componentObjectHandle = Activator.CreateInstance(_assemblyName, meta.ComponentName);
            var componentObj = componentObjectHandle.Unwrap();

            foreach (var field in meta.Fields)
            {
                //var propInfo = compType.GetField("speed");
                //propInfo.SetValue(componentObj, 0407);
                var value = field.GetValue();
                if (value == null)
                    continue;

                var fieldInfo = compType.GetField(field.Name);
                fieldInfo.SetValue(componentObj, value);
            }

            //TODO: if can't invoke with null arg implement second AddComponentNoReturn without second argument
            addComponentInfoGen.Invoke(_world, new object[] { _entity.GetId(), componentObj });
        }
    }
}
