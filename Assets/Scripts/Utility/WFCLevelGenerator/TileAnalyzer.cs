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

    public void Add(ETileSide side, int neighbourId, float neighbourRotation)
    {
        if (!Neighbours.ContainsKey(side))
            Neighbours[side] = new List<ProbableEntry>();
        var neighboursOnSide = Neighbours[side];
        var neighbourIdx = neighboursOnSide.FindIndex(neighbour => neighbour.Entry.Id == neighbourId &&
                                                             Mathf.Abs(neighbour.Entry.YRotation - neighbourRotation) <
                                                             float.Epsilon);
        
        if (neighbourIdx < 0)
        {
            neighboursOnSide.Add(new ProbableEntry(new PatternEntry(neighbourId, neighbourRotation), 1));
        }
        else
        {
            var neighbour = neighboursOnSide[neighbourIdx];
            neighbour.Weight++;
            neighboursOnSide[neighbourIdx] = neighbour;
        }
    }
}

public class PatternEntryEqualityComparer : IEqualityComparer<PatternEntry>
{
    public bool Equals(PatternEntry x, PatternEntry y) => 
        x.Id == y.Id && Mathf.Abs(x.YRotation - y.YRotation) < float.Epsilon;

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

    public static readonly PatternEntry PseudoEntry = new PatternEntry
    {
        Id = -1,
        YRotation = 0,
    };
    
    public PatternEntry(int tileId, float yRotation)
    {
        Id = tileId;
        YRotation = yRotation % 360;
    }
}

[RequireComponent(typeof(TilePlacer))]
[RequireComponent(typeof(TilePalette))]
public class TileAnalyzer : MonoBehaviour
{
    public static readonly PatternEntryEqualityComparer EntryComparer;

    static TileAnalyzer() => EntryComparer = new PatternEntryEqualityComparer();
    
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
        PatternFilePath = Application.persistentDataPath + "/pattern";
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
        if (Input.GetMouseButton(1))
            return;
        
        if (Input.GetKeyDown(KeyCode.A))
        {
            if (!Input.GetKey(KeyCode.LeftShift))
                _pattern.Clear();
            Analyze();
            var patternList = PatternDictToList(_pattern);
            var patternJson = JsonConvert.SerializeObject(patternList);
            MiscUtils.WriteStringToFile(PatternFilePath, patternJson, false);
            Debug.Log("Pattern saved at: " + PatternFilePath);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                var presetJson = MiscUtils.ReadStringFromFile(Application.persistentDataPath + "/preset");
                var preset = JsonConvert.DeserializeObject<Preset>(presetJson);
                if (_placer.Dimension != preset.Dimension)
                {
                    Debug.LogError("can't load preset due to dimension mismatch");
                }
                else
                {
                    foreach (var presetEntry in preset.Entries)
                    {
                        var gridPos = presetEntry.Item1;
                        var patternEntry = presetEntry.Item2;
                        _placer.PlaceTile(patternEntry.Id, WFCHelper.GridPosToPos(gridPos, preset.Dimension), patternEntry.YRotation, true);
                    }
                }
            }
            else
                LoadPattern(PatternFilePath);
        }
        if (Input.GetKeyDown(KeyCode.P))
            SavePreset(Application.persistentDataPath + "/preset");
    }

    private struct Preset
    {
        public int Dimension;
        public List<Tuple<Vector2Int, PatternEntry>> Entries;
    }
    
    private void SavePreset(string path)
    {
        var dimension = _placer.Dimension;
        var placedEntries = new List<Tuple<Vector2Int, PatternEntry>>();
        foreach (var placedTile in _placer.PlacedTiles)
        {
            var gridPos = WFCHelper.PosToGridPos(placedTile.Key, dimension);
            float rotation;
            switch (placedTile.Value.RotationType)
            {
                case ETileRotation.No:
                    rotation = 0;
                    break;
                case ETileRotation.Two:
                    rotation = placedTile.Value.GetTileRotation() % 180;
                    break;
                case ETileRotation.Four:
                     rotation = placedTile.Value.GetTileRotation() % 360;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            var entry = new PatternEntry(placedTile.Value.TileId, rotation);
            placedEntries.Add(Tuple.Create(gridPos, entry));
        }
        
        var pattern = new Preset
        {
            Dimension = dimension,
            Entries = placedEntries
        };

        var patternJson = JsonConvert.SerializeObject(pattern);
        MiscUtils.WriteStringToFile(path, patternJson, false);
        Debug.Log("Preset saved at: " + path);
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
            for (int i = 0; i < 8; i++)
            {
                var side = (ETileSide)i;
                var bias = WFCHelper.BiasPerSide[side];
                var neighbourPos = pos;
                neighbourPos.x += bias.x * _placer.SnapSize;
                neighbourPos.z += bias.y * _placer.SnapSize;
                var gridPos = WFCHelper.PosToGridPos(neighbourPos, _placer.Dimension);
                if (gridPos.x < 0 || gridPos.x >= _placer.Dimension || gridPos.y < 0 || gridPos.y >= _placer.Dimension)
                {
                    var pseudoEntry = PatternEntry.PseudoEntry;
                    if (!_pattern.ContainsKey(pseudoEntry))
                        _pattern.Add(pseudoEntry, new ProbableNeighbours());
                    var oppositeSide = WFCHelper.GetOppositeSide(side);
                    _pattern[pseudoEntry].Add(oppositeSide, tile.TileId, tile.GetTileRotation());
                }
                else if (_placer.PlacedTiles.ContainsKey(neighbourPos))
                {
                    var neighbour = _placer.PlacedTiles[neighbourPos];
                    //TODO: refactor all the stuff with tile rotations of different tile types here and in SavePreset too
                    switch (tile.RotationType)
                    {
                        case ETileRotation.No:
                            {
                                var entry = new PatternEntry(tile.TileId, 0);
                                if (!_pattern.ContainsKey(entry))
                                    _pattern.Add(entry, new ProbableNeighbours());
                                for (int j = 0; j < 4; j++)
                                {
                                    var neighbourRotation = neighbour.RotationType switch
                                    {
                                        ETileRotation.No => 0,
                                        ETileRotation.Two => (neighbour.GetTileRotation() + j * 90) % 180,
                                        ETileRotation.Four => (neighbour.GetTileRotation() + j * 90) % 360,
                                        _ => throw new ArgumentOutOfRangeException()
                                    };
                                    
                                    var rotatedSide = WFCHelper.TurnSide(side, j * 2);
                                    _pattern[entry].Add(rotatedSide, neighbour.TileId, neighbourRotation);
                                }
                            }
                            break;
                        case ETileRotation.Two:
                            for (int j = 0; j < 2; j++)
                            {
                                var rotation = (tile.GetTileRotation() + j*90) % 180;
                                var entry = new PatternEntry(tile.TileId, rotation);
                                if (!_pattern.ContainsKey(entry))
                                    _pattern.Add(entry, new ProbableNeighbours());
                                
                                var neighbourRotation = neighbour.RotationType switch
                                {
                                    ETileRotation.No => 0,
                                    ETileRotation.Two => (neighbour.GetTileRotation() + j * 90) % 180,
                                    ETileRotation.Four => (neighbour.GetTileRotation() + j * 90) % 360,
                                    _ => throw new ArgumentOutOfRangeException()
                                };
                                
                                var rotatedSide = WFCHelper.TurnSide(side, j * 2);
                                _pattern[entry].Add(rotatedSide, neighbour.TileId, neighbourRotation);

                                if (neighbour.RotationType == ETileRotation.Four)
                                    neighbourRotation = (neighbourRotation + 180) % 360;
                                _pattern[entry].Add(WFCHelper.GetOppositeSide(rotatedSide), neighbour.TileId, neighbourRotation);
                            }
                            break;
                        case ETileRotation.Four:
                            for (int j = 0; j < 4; j++)
                            {
                                var rotation = (tile.GetTileRotation() + j*90) % 360;
                                var entry = new PatternEntry(tile.TileId, rotation);
                                if (!_pattern.ContainsKey(entry))
                                    _pattern.Add(entry, new ProbableNeighbours());

                                var neighbourRotation = neighbour.RotationType switch
                                {
                                    ETileRotation.No => 0,
                                    ETileRotation.Two => (neighbour.GetTileRotation() + j * 90) % 180,
                                    ETileRotation.Four => (neighbour.GetTileRotation() + j * 90) % 360,
                                    _ => throw new ArgumentOutOfRangeException()
                                };

                                var rotatedSide = WFCHelper.TurnSide(side, j * 2);
                                _pattern[entry].Add(rotatedSide, neighbour.TileId, neighbourRotation);
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }
    }
}
