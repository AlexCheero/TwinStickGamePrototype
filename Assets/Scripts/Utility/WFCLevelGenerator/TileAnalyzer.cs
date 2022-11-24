using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using WFC;

public class ProbableNeighbours
{
    public Dictionary<ETileSide, List<ProbableEntry>> Neighbours;

    public ProbableNeighbours()
    {
        Neighbours = new Dictionary<ETileSide, List<ProbableEntry>>();
    }

    public void Add(ETileSide side, Tile tile)
    {
        if (!Neighbours.ContainsKey(side))
            Neighbours[side] = new List<ProbableEntry>();
        var neighboursOnSide = Neighbours[side];
        var tileId = neighboursOnSide.FindIndex(neighbour => neighbour.Entry.Id == tile.TileId &&
                                                             Mathf.Abs(neighbour.Entry.YRotation - tile.transform.eulerAngles.y) >
                                                             TileAnalyzer.RotationTolerance);
        
        var minChance = neighboursOnSide.Count > 0 ? neighboursOnSide.Min(neighbour => neighbour.Chance) : 1;
        var countOverall = (int)neighboursOnSide.Sum(e => e.Chance / minChance);
        
        if (tileId < 0)
        {
            for (int i = 0; i < neighboursOnSide.Count; i++)
            {
                var neighbour = neighboursOnSide[i];
                var count = countOverall * neighbour.Chance;
                neighbour.Chance = count / (countOverall + 1);
                neighboursOnSide[i] = neighbour;
            }
            
            var rotation = tile.transform.eulerAngles.y;
            rotation = Mathf.Clamp(rotation, 0, 359);
#if DEBUG
            var entry = new PatternEntry { Id = tile.TileId, YRotation = rotation, name = tile.name };
#else
            var entry = new PatternEntry { Id = tile.TileId, YRotation = rotation };
#endif
            
            neighboursOnSide.Add(new ProbableEntry(entry, 1.0f /  (countOverall + 1)));
        }
        else
        {
            for (int i = 0; i < neighboursOnSide.Count; i++)
            {
                var neighbour = neighboursOnSide[i];
                var count = countOverall * neighbour.Chance;
                if (i == tileId)
                    count += 1;
                neighbour.Chance = count / (countOverall + 1);
                neighboursOnSide[i] = neighbour;
            }
        }
        
#if DEBUG
        var chanceOverall = neighboursOnSide.Sum(neighbour => neighbour.Chance);
        if (Mathf.Abs(chanceOverall - 1) > TileAnalyzer.RotationTolerance)
            throw new Exception("overall chance of tile neighbours is not equal to 1");
#endif
    }
}

public struct PatternEntry
{
    public int Id;
    public float YRotation;
#if DEBUG
    public string name;
#endif

    public override int GetHashCode()
    {
        var hash = 17 * 23 + Id;
        hash = hash * 23 + (int)YRotation;
        return hash;
    }
}

[RequireComponent(typeof(TilePlacer))]
[RequireComponent(typeof(TilePalette))]
public class TileAnalyzer : MonoBehaviour
{
    public const float RotationTolerance = 0.01f;
    
    [SerializeField]
    private bool _isEightDirectionAnalyze;
    
    private TilePlacer _placer;
    private TilePalette _palette;
    
    [NonSerialized]
    public Dictionary<PatternEntry, ProbableNeighbours> Pattern;

    private List<Tuple<PatternEntry, ProbableNeighbours>> _pattern;

    private void PatternToList()
    {
        _pattern = new List<Tuple<PatternEntry, ProbableNeighbours>>(Pattern.Count);
        foreach (var pair in Pattern)
            _pattern.Add(Tuple.Create(pair.Key, pair.Value));
    }

    private void ListToPattern()
    {
        Pattern = new Dictionary<PatternEntry, ProbableNeighbours>(_pattern.Count);
        foreach (var tuple in _pattern)
            Pattern[tuple.Item1] = tuple.Item2;
    }

    private string PatternFilePath;
    
    void Awake()
    {
        PatternFilePath = Application.persistentDataPath + "/pattern";
    }
    
    void Start()
    {
        _placer = GetComponent<TilePlacer>();
        _palette = GetComponent<TilePalette>();

        //_pattern init should be in Start when all duplicates are already removed from palette
        Pattern = new Dictionary<PatternEntry, ProbableNeighbours>(_palette.Palette.Count);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Analyze();
            PatternToList();
            var patternJson = JsonConvert.SerializeObject(_pattern);
            MiscUtils.WriteStringToFile(PatternFilePath, patternJson, false);
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            var patternJson = MiscUtils.ReadStringFromFile(PatternFilePath);
            _pattern = JsonConvert.DeserializeObject<List<Tuple<PatternEntry, ProbableNeighbours>>>(patternJson);
            ListToPattern();
        }
    }

    private void Analyze()
    {
        foreach (var tile in _placer.PlacedTiles.Values)
        {
            var pos = tile.transform.position;
            for (int i = 0; i < 9; i++)
            {
                if (!_isEightDirectionAnalyze && i % 2 == 0)
                    continue;
                var side = (ETileSide)i;
                if (side == ETileSide.Center)
                    continue;
                
                var xDelta = i % 3 - 1;
                var yDelta = i / 3 - 1;
                
                var neighbourPos = pos;
                neighbourPos.x += xDelta * _placer.SnapSize;
                neighbourPos.z += yDelta * _placer.SnapSize;
                if (!_placer.PlacedTiles.ContainsKey(neighbourPos))
                    continue;
                var neighbour = _placer.PlacedTiles[neighbourPos];

#if DEBUG
                var entry = new PatternEntry { Id = tile.TileId, YRotation = tile.transform.eulerAngles.y, name = tile.name  };
#else
                var entry = new PatternEntry { Id = tile.TileId, YRotation = tile.transform.eulerAngles.y  };
#endif
                if (!Pattern.ContainsKey(entry))
                    Pattern.Add(entry, new ProbableNeighbours());
                Pattern[entry].Add(side, neighbour);
            }
        }
    }
}
