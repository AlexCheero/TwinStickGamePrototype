using ECS;
using UnityEngine;

public class CharacterConvertible : ECSConvertible
{
    [SerializeField]
    private float _speed;

    [SerializeField]
    private float _health;

    public override void ConvertToEntity(EcsWorld world)
    {
        var entity = world.Create();
        world.AddTag<PlayerTag>(entity);
        world.AddComponent(entity, transform);
        world.AddComponent(entity, new SpeedComponent { speed = _speed });
        world.AddComponent(entity, new HealthComponent { health = _health });
    }
}
