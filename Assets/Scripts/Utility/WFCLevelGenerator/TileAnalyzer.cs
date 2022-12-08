using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using WFC;

using PatternList = System.Collections.Generic.List<System.Tuple<PatternEntry, ProbableNeighbours>>;
using PatternDict = System.Collections.Generic.Dictionary<PatternEntry, ProbableNeighbours>;

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
        
        if (tileId < 0)
        {
            neighboursOnSide.Add(new ProbableEntry(new PatternEntry(tile), 1));
        }
        else
        {
            var neighbour = neighboursOnSide[tileId];
            neighbour.Weight++;
            neighboursOnSide[tileId] = neighbour;
        }
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

    private TilePlacer _placer;
    private TilePalette _palette;
    
    [NonSerialized]
    private PatternDict _pattern;

    public ProbableNeighbours this[PatternEntry e] => _pattern[e];
    public bool IsPatternInited => _pattern != null;
    public bool Contains(PatternEntry e) => _pattern.ContainsKey(e);
    public ICollection<PatternEntry> Keys => _pattern.Keys;

    private PatternList PatternDictToList(PatternDict dict)
    {
        var list = new PatternList(dict.Count);
        foreach (var pair in dict)
            list.Add(Tuple.Create(pair.Key, pair.Value));
        return list;
    }

    private PatternDict PatternListToDict(PatternList list)
    {
        var dict = new PatternDict(list.Count, EntryComparer);
        foreach (var tuple in list)
            dict[tuple.Item1] = tuple.Item2;
        return dict;
    }

    private string PatternFilePath;
    
    void Awake()
    {
        PatternFilePath = Application.persistentDataPath + "/pattern3";
    }
    
    void Start()
    {
        _placer = GetComponent<TilePlacer>();
        _palette = GetComponent<TilePalette>();

        //_pattern init should be in Start when all duplicates are already removed from palette
        _pattern = new Dictionary<PatternEntry, ProbableNeighbours>(_palette.Palette.Count, EntryComparer);

        LoadPattern(PatternFilePath);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            _pattern.Clear();
            Analyze();
            var patternList = PatternDictToList(_pattern);
            var patternJson = JsonConvert.SerializeObject(patternList);
            MiscUtils.WriteStringToFile(PatternFilePath, patternJson, false);
        }
        if (Input.GetKeyDown(KeyCode.L))
            LoadPattern(PatternFilePath);
    }

    private void LoadPattern(string path)
    {
        var patternJson = MiscUtils.ReadStringFromFile(path);
        if (string.IsNullOrEmpty(patternJson))
            Debug.LogError("can't load pattern json");
        else
        {
            var list = JsonConvert.DeserializeObject<List<Tuple<PatternEntry, ProbableNeighbours>>>(patternJson);
            _pattern = PatternListToDict(list);
        }
    }

    private void Analyze()
    {
        foreach (var tile in _placer.PlacedTiles.Values)
        {
            var pos = tile.transform.position;
            var rotation = tile.transform.eulerAngles.y;
            rotation = Mathf.Clamp(rotation, 0, 359);
            WFCHelper.ForEachSide((side, x, y) =>
            {
                var neighbourPos = pos;
                neighbourPos.x += x * _placer.SnapSize;
                neighbourPos.z += y * _placer.SnapSize;
                if (!_placer.PlacedTiles.ContainsKey(neighbourPos))
                {
                    var pseudoEntry = PatternEntry.PseudoEntry;
                    if (!_pattern.ContainsKey(pseudoEntry))
                        _pattern.Add(pseudoEntry, new ProbableNeighbours());
                    var oppositeSide = WFCHelper.GetOppositeSide(side);
                    _pattern[pseudoEntry].Add(oppositeSide, tile);
                    return;
                }
                var neighbour = _placer.PlacedTiles[neighbourPos];

                var entry = new PatternEntry(tile);
                if (!_pattern.ContainsKey(entry))
                    _pattern.Add(entry, new ProbableNeighbours());
                _pattern[entry].Add(side, neighbour);
            }, _isEightDirectionAnalyze);
        }
    }
}
