using ECS;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEngine;

[Serializable]
public struct ComponentFieldMeta//TODO: handle case when some ref types used as component
{
    //TODO: define different access modifiers for UNITY_EDITOR (and hide some getters)
    public string TypeName;
    public string Name;
    public string ValueRepresentation;
    public Component UnityComponent;

    public object GetValue()
    {
        bool isRepresentationNotEmpty = ValueRepresentation != null && ValueRepresentation.Length > 0;
        //TODO: move all these typeof to single place, possibly to implement code generation in future
        if (TypeName == typeof(int).Name)
            return isRepresentationNotEmpty ? int.Parse(ValueRepresentation) : 0;
        else if (TypeName == typeof(float).Name)
            return isRepresentationNotEmpty ? float.Parse(ValueRepresentation, CultureInfo.InvariantCulture) : 0;
        else if (TypeName == typeof(Vector3).Name)
            return isRepresentationNotEmpty ? ParseVector3(ValueRepresentation) : Vector3.zero;
        else
        {
            var type = typeof(Component).Assembly.GetType(TypeName);
            if (typeof(Component).IsAssignableFrom(type))
            {
                return UnityComponent;
            }
            else
            {
                Debug.LogError("Wrong field meta Type");
                return null;
            }
        }
    }

    public void SetValue(object value)
    {
        Debug.LogWarning("Not implemented yet!");
        if (TypeName == typeof(int).Name)
            return;
        else if (TypeName == typeof(float).Name)
            return;
        else if (TypeName == typeof(Vector3).Name)
            return;
        else
        {
            var type = typeof(Component).Assembly.GetType(TypeName);
            if (typeof(Component).IsAssignableFrom(type))
            {
                UnityComponent = (Component)value;
            }
            else
            {
                Debug.LogError("Wrong field meta Type");
                return;
            }
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
    public static readonly string Components = "Components";
    public static readonly string Tags = "Tags";

    private Entity _entity;
    private EcsWorld _world;

    public static Type[] ComponentTypes;

    static EntityView()
    {
        ComponentTypes = Assembly.GetAssembly(typeof(EntityView)).GetTypes()
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
            Fields = GetComponentTypeFields(componentName),
            IsExpanded = false
        };
    }

    public ComponentFieldMeta[] GetComponentTypeFields(string componentName)
    {
        var compType = GetComponentTypeByName(componentName);
        var fields = compType.GetFields();
        var result = new ComponentFieldMeta[fields.Length];
        for (int i = 0; i < fields.Length; i++)
        {
            var field = fields[i];
            var fieldType = field.FieldType;
            if (!fieldType.IsValueType && !typeof(Component).IsAssignableFrom(fieldType))
            {
                Debug.LogError("wrong component field type. fields should only be pods or derives UnityEngine.Component");
                return new ComponentFieldMeta[0];
            }

            result[i] = new ComponentFieldMeta
            {
                TypeName = fieldType.FullName,
                Name = field.Name,
                ValueRepresentation = string.Empty,
                UnityComponent = null
            };
        }
        return result;
    }
#endif

    private static Type GetComponentTypeByName(string componentName)
    {
        foreach (var compType in ComponentTypes)
        {
            if (compType.Name == componentName)
                return compType;
        }

        return null;
    }

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
                fieldInfo.SetValue(componentObj, value);
            }

            //TODO: if can't invoke with null arg implement second AddComponentNoReturn without second argument
            GetComponentParams[0] = _entity.GetId();
            GetComponentParams[1] = componentObj;
            addComponentInfoGen.Invoke(_world, GetComponentParams);
        }
    }
}
