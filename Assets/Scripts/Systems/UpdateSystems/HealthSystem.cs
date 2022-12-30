using Components;
using ECS;
using Tags;
using UnityEngine;
using UnityEngine.SceneManagement;

[System(ESystemCategory.Update)]
public class HealthSystem : EcsSystem
{
    private int _filterId;

    public HealthSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<HealthComponent>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            if (world.GetComponent<HealthComponent>(id).health > 0)
                continue;

            world.Add<DeadTag>(id);
            if (world.Have<EnemyTag>(id))
                MiscUtils.AddScore(Constants.PointsPerKill);
            else if (world.Have<PlayerTag>(id))
            {
                if (EntityStashHolder.IsCreated)
                    Object.Destroy(EntityStashHolder.Instance);
                SceneManager.LoadScene(Constants.MainMenu);
            }
        }
    }
}
