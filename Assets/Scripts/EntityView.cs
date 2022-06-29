using ECS;
using System;
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
        if (TypeName == typeof(int).FullName)
            return isRepresentationNotEmpty ? int.Parse(ValueRepresentation) : 0;
        else if (TypeName == typeof(float).FullName)
            return isRepresentationNotEmpty ? float.Parse(ValueRepresentation, CultureInfo.InvariantCulture) : 0;
        else if (TypeName == typeof(Vector3).FullName)
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

#if UNITY_EDITOR
    public bool SetValue(object value)
    {
        var previousRepresentation = ValueRepresentation;
        var previousComponent = UnityComponent;
        
        if (TypeName == typeof(int).FullName)
        {
            ValueRepresentation = value.ToString();
        }
        else if (TypeName == typeof(float).FullName)
        {
            ValueRepresentation = ((float)value).ToString(CultureInfo.InvariantCulture);
        }
        else if (TypeName == typeof(Vector3).FullName)
        {
            var vec = (Vector3)value;
            ValueRepresentation = vec.x + " " + vec.y + " " + vec.z;
        }
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
            }
        }

        return previousRepresentation != ValueRepresentation || previousComponent != UnityComponent;
    }
#endif

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
    public const string Components = "Components";
    public const string Tags = "Tags";

    private Entity _entity;
    private EcsWorld _world;

    public static Type[] EcsComponentTypes;

    static EntityView()
    {
        EcsComponentTypes = typeof(EntityView).Assembly.GetTypes()
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

    public bool AddComponent(string componentName)
    {
        foreach (var meta in _metas)
        {
            if (meta.ComponentName == componentName)
                return false;
        }

        Array.Resize(ref _metas, _metas.Length + 1);
        _metas[_metas.Length - 1] = new ComponentMeta
        {
            ComponentName = componentName,
            Fields = GetComponentTypeFields(componentName),
            IsExpanded = false
        };

        return true;
    }

    public ComponentFieldMeta[] GetComponentTypeFields(string componentName)
    {
        var compType = GetEcsComponentTypeByName(componentName);
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

    private static Type GetEcsComponentTypeByName(string componentName)
    {
        foreach (var compType in EcsComponentTypes)
        {
            if (compType.FullName == componentName)
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
            var compType = GetEcsComponentTypeByName(meta.ComponentName);
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
