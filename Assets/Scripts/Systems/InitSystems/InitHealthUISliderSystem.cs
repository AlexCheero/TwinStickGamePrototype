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
        _filterId = world.RegisterFilter(new BitMask(Id<HealthLimitsComponent>(), Id<HealthUISlider>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            Debug.Log("A");
            var slider = world.GetComponent<HealthUISlider>(id).slider;
            var healthLimits = world.GetComponent<HealthLimitsComponent>(id);
            slider.minValue = 0;
            slider.maxValue = healthLimits.maxHealth;
            slider.value = healthLimits.initialHealth;
        }
    }
}