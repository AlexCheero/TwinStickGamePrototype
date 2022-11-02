using Components;
using ECS;

//choose system type here
[System(ESystemCategory.Update)]
public class HealthUISliderSystem : EcsSystem
{
    private int _filterId;

    public HealthUISliderSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<HealthComponent>(), Id<HealthUISlider>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            world.GetComponent<HealthUISlider>(id).slider.value = world.GetComponent<HealthComponent>(id).health;
        }
    }
}