using Components;
using ECS;
using UnityEngine;

//choose system type here
[System(ESystemCategory.Init)]
public class InitHealthUISliderSystem : EcsSystem
{
    private int _filterId;

    public InitHealthUISliderSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<HealthLimitsComponent>(), Id<HealthComponent>(), Id<HealthUISlider>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            var slider = world.GetComponent<HealthUISlider>(id).slider;
            slider.minValue = 0;
            slider.maxValue = world.GetComponent<HealthLimitsComponent>(id).maxHealth;
            slider.value = world.GetComponent<HealthComponent>(id).health;
        }
    }
}