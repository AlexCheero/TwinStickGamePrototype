using ECS;
using Tags;
using UnityEngine;
using WFC;

//choose system type here
[System(ESystemCategory.Init)]
public class GenerateLevelSystem : EcsSystem
{
    private int _playerFilterId;

    public GenerateLevelSystem(EcsWorld world)
    {
        _playerFilterId = world.RegisterFilter(new BitMask(Id<PlayerTag>(), Id<Transform>()));
    }

    public override void Tick(EcsWorld world)
    {
        var generatorObject = GameObject.Find("WFCGenerator");
        //generatorObject.GetComponent<TileAnalyzer>().LoadPreset();
        generatorObject.GetComponent<WFCGenerator>().GenerateLevel();

        var levelDimension = generatorObject.GetComponent<TilePlacer>().Dimension;
        var spawnPosition = WFCHelper.GridPosToPos(new Vector2Int(levelDimension - 3, levelDimension - 3), levelDimension);
        spawnPosition.y += 1.0f;

        foreach (var id in world.Enumerate(_playerFilterId))
        {
            var transform = world.GetComponent<Transform>(id);
            transform.position = spawnPosition;
            transform.gameObject.SetActive(true);
            break;
        }
    }
}