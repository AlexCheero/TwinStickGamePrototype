using ECS;
using UnityEngine;

public abstract class ECSConvertible : MonoBehaviour
{
    public abstract void ConvertToEntity(EcsWorld world);
}
