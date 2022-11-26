using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using WFC;

public class ProbableNeighbours
{
    //TODO: make private
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
                                                             Mathf.Abs(neighbour.Entry.YRotation - tile.transform.eulerAngles.y) <
                                                             PatternEntryEqualityComparer.RotationTolerance);
        
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
            
            neighboursOnSide.Add(new ProbableEntry(new PatternEntry(tile), 1.0f /  (countOverall + 1)));
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
        if (Mathf.Abs(chanceOverall - 1) > PatternEntryEqualityComparer.RotationTolerance)
            throw new Exception("overall chance of tile neighbours is not equal to 1");
#endif
    }
}

public class PatternEntryEqualityComparer : IEqualityComparer<PatternEntry>
{
    public const float RotationTolerance = 0.01f;
    public bool Equals(PatternEntry x, PatternEntry y) => 
        x.Id == y.Id && Mathf.Abs(x.YRotation - y.YRotation) < RotationTolerance;

    public int GetHashCode(PatternEntry obj)
    {
        var hash = 17 * 23 + obj.Id;
        hash = hash * 23 + (int)obj.YRotation;
        return hash;
    }
}

public struct PatternEntry
{
    public int Id;
    public float YRotation;
#if DEBUG
    private string name;
#endif

    public static readonly PatternEntry PseudoEntry = new PatternEntry
    {
        Id = -1,
        YRotation = 0,
#if DEBUG
        name = "PseudoTile"
#endif
    };
    
    public PatternEntry(Tile tile)
    {
        Id = tile.TileId;
        YRotation = tile.transform.eulerAngles.y % 360;
#if DEBUG
        name = tile.name;
#endif
    }
}

[RequireComponent(typeof(TilePlacer))]
[RequireComponent(typeof(TilePalette))]
public class TileAnalyzer : MonoBehaviour
{
    public static readonly PatternEntryEqualityComparer EntryComparer;

    static TileAnalyzer() => EntryComparer = new PatternEntryEqualityComparer();
    
    [SerializeField]
    private bool _isEightDirectionAnalyze;

    public bool IsEightDirectionAnalyze => _isEightDirectionAnalyze;
    
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
        Pattern = new Dictionary<PatternEntry, ProbableNeighbours>(_pattern.Count, EntryComparer);
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
        Pattern = new Dictionary<PatternEntry, ProbableNeighbours>(_palette.Palette.Count, EntryComparer);
        
        var patternJson = MiscUtils.ReadStringFromFile(PatternFilePath);
        if (!string.IsNullOrEmpty(patternJson))
        {
            _pattern = JsonConvert.DeserializeObject<List<Tuple<PatternEntry, ProbableNeighbours>>>(patternJson);
            ListToPattern();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Pattern.Clear();
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
            WFCHelper.ForEachSide(_isEightDirectionAnalyze, (side, x, y) =>
            {
                var neighbourPos = pos;
                neighbourPos.x += x * _placer.SnapSize;
                neighbourPos.z += y * _placer.SnapSize;
                if (!_placer.PlacedTiles.ContainsKey(neighbourPos))
                {
                    var pseudoEntry = PatternEntry.PseudoEntry;
                    if (!Pattern.ContainsKey(pseudoEntry))
                        Pattern.Add(pseudoEntry, new ProbableNeighbours());
                    var oppositeSide = (ETileSide)(8 - (int)side);
                    Pattern[pseudoEntry].Add(oppositeSide, tile);
                    return;
                }
                var neighbour = _placer.PlacedTiles[neighbourPos];

                var entry = new PatternEntry(tile);
                if (!Pattern.ContainsKey(entry))
                    Pattern.Add(entry, new ProbableNeighbours());
                Pattern[entry].Add(side, neighbour);
            });
        }
    }
}
