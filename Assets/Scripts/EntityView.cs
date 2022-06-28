using ECS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public enum EFieldType
{
    Int,
    Float,
    Vec3,
    //TODO: can't determine type of field when adding in inspector, so add just as "Component",
    //      and then determine in wether prefab or scene go in inspector
    //TODO: maybe could just load EntityView, instead of generic components
    //SceneGO,//if go.scene.IsValid() then make a hierarchy by traversing all parents
    //Prefab//else find the prefab path
    GO
}

[Serializable]
public struct ComponentFieldMeta
{
    //TODO: define different access modifiers for UNITY_EDITOR (and hide some getters)
    public EFieldType Type;
    public string TypeName;
    public string Name;
    public string ValueRepresentation;
    public GameObject GO;

    public object GetValue()
    {
        if (ValueRepresentation == null || ValueRepresentation.Length == 0)
            return null;

        switch (Type)
        {
            case EFieldType.Int:
                return int.Parse(ValueRepresentation);
            case EFieldType.Float:
                return float.Parse(ValueRepresentation);
            case EFieldType.Vec3:
                return ParseVector3(ValueRepresentation);
            case EFieldType.GO:
                return GO;
            default:
                Debug.LogError("Wrong field meta Type");
                return null;
        }
    }

    public void SetValue(object value)
    {
        switch (Type)
        {
            case EFieldType.Int:
                break;
            case EFieldType.Float:
                break;
            case EFieldType.Vec3:
                break;
            case EFieldType.GO:
                break;
            default:
                Debug.LogError("Wrong field meta Type");
                break;
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
    //TODO: maybe use type names as is and get rid of EFieldType enum
    public static readonly Dictionary<string, EFieldType> NameToFieldTypeMap = new Dictionary<string, EFieldType>
    {
        { typeof(float).Name, EFieldType.Float },
        { typeof(Vector3).Name, EFieldType.Vec3 },
        { typeof(int).Name, EFieldType.Int },
    };

    public static readonly string Components = "Components";
    public static readonly string Tags = "Tags";

    private Entity _entity;
    private EcsWorld _world;

    private static Type[] _componentTypes;

    static EntityView()
    {
        _componentTypes = Assembly.GetAssembly(typeof(EntityView)).GetTypes()
            .Where((t) => t.Namespace == Components || t.Namespace == Tags).ToArray();
    }

    [SerializeField]
    private ComponentMeta[] _metas;

#if UNITY_EDITOR
    public int MetasLength { get => _metas.Length; }
    public ref ComponentMeta GetMeta(int i) => ref _metas[i];
    public void RemoveMetaAt(int idx)
    {
        var newLength = _metas.Length - 1;
        for (int i = idx; i < newLength; i++)
            _metas[i] = _metas[i + 1];
        Array.Resize(ref _metas, newLength);
    }

    public void AddComponent(string componentName)
    {
        foreach (var meta in _metas)
        {
            if (meta.ComponentName == componentName)
                return;
        }

        Array.Resize(ref _metas, _metas.Length + 1);
        _metas[_metas.Length - 1] = new ComponentMeta
        {
            ComponentName = componentName,
            Fields = GetComponentFields(componentName),
            IsExpanded = false
        };
    }

    public ComponentFieldMeta[] GetComponentFields(string componentName)
    {
        var compType = GetComponentTypeByName(componentName);
        var fields = compType.GetFields();
        var result = new ComponentFieldMeta[fields.Length];
        for (int i = 0; i < fields.Length; i++)
        {
            var field = fields[i];
            var fieldTypeName = field.FieldType.Name;
            EFieldType metaFieldType;
            if (NameToFieldTypeMap.ContainsKey(fieldTypeName))
                metaFieldType = NameToFieldTypeMap[field.FieldType.Name];
            else
                metaFieldType = EFieldType.GO;

            result[i] = new ComponentFieldMeta
            {
                Type = metaFieldType,
                Name = field.Name,
                ValueRepresentation = string.Empty
            };
        }
        return result;
    }
#endif

    private Type GetComponentTypeByName(string componentName)
    {
        foreach (var compType in _componentTypes)
        {
            if (compType.Name == componentName)
                return compType;
        }

        return null;
    }

    private static readonly object[] EmptyParams = { };
    private static readonly object[] GetComponentParams = { null, null };
    public void InitAsEntity(EcsWorld world)
    {
        _world = world;

        var entityId = _world.Create();
        _entity = _world.GetById(entityId);

        MethodInfo addComponentInfo = typeof(EcsWorld).GetMethod("AddComponentNoReturn");

        foreach (var meta in _metas)
        {
            var compType = GetComponentTypeByName(meta.ComponentName);
#if DEBUG
            if (compType == null)
                throw new Exception("can't find component type");
#endif
            MethodInfo addComponentInfoGen = addComponentInfo.MakeGenericMethod(compType);

            var componentObj = Activator.CreateInstance(compType);

            foreach (var field in meta.Fields)
            {
                var value = field.GetValue();
                if (value == null)
                    continue;

                var fieldInfo = compType.GetField(field.Name);
                switch (field.Type)
                {
                    case EFieldType.Float:
                    case EFieldType.Int:
                    case EFieldType.Vec3:
                        fieldInfo.SetValue(componentObj, value);
                        break;
                    case EFieldType.GO:
                        MethodInfo getUnityComponentInfo = typeof(GameObject).GetMethod("GetComponent");
                        getUnityComponentInfo = addComponentInfo.MakeGenericMethod(fieldInfo.FieldType);
                        getUnityComponentInfo.Invoke(field.GO, EmptyParams);
                        break;
                }
                
            }

            //TODO: if can't invoke with null arg implement second AddComponentNoReturn without second argument
            GetComponentParams[0] = _entity.GetId();
            GetComponentParams[1] = componentObj;
            addComponentInfoGen.Invoke(_world, GetComponentParams);
        }
    }
}
