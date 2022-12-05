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
        var tileId = neighboursOnSide.FindIndex(neighbour => neighbour.Id == tile.TileId);
        
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
            
            neighboursOnSide.Add(new ProbableEntry(tile.TileId, 1.0f /  (countOverall + 1)));
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
        if (Mathf.Abs(chanceOverall - 1) > 0.001f)
            throw new Exception("overall chance of tile neighbours is not equal to 1");
#endif
    }
}

[RequireComponent(typeof(TilePlacer))]
[RequireComponent(typeof(TilePalette))]
public class TileAnalyzer : MonoBehaviour
{
    [SerializeField]
    private bool _isEightDirectionAnalyze;

    private TilePlacer _placer;
    private TilePalette _palette;
    
    [NonSerialized]
    private Dictionary<int, ProbableNeighbours> _pattern;

    public ProbableNeighbours this[int id] => _pattern[id];
    public bool IsPatternInited => _pattern != null;
    public bool Contains(int id) => _pattern.ContainsKey(id);
    public ICollection<int> Keys => _pattern.Keys;

    private string PatternFilePath;
    
    void Awake()
    {
#if UNITY_EDITOR
        PatternFilePath = "pattern";
#else
        PatternFilePath = Application.persistentDataPath + "/pattern";
#endif
    }
    
    void Start()
    {
        _placer = GetComponent<TilePlacer>();
        _palette = GetComponent<TilePalette>();

        //_pattern init should be in Start when all duplicates are already removed from palette
        _pattern = new Dictionary<int, ProbableNeighbours>(_palette.Palette.Count);

        LoadPattern(PatternFilePath);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            _pattern.Clear();
            Analyze();
            var patternJson = JsonConvert.SerializeObject(_pattern);
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
            _pattern = JsonConvert.DeserializeObject<Dictionary<int, ProbableNeighbours>>(patternJson);
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
                    if (!_pattern.ContainsKey(WFCHelper.PseudoTileId))
                        _pattern.Add(WFCHelper.PseudoTileId, new ProbableNeighbours());
                    var oppositeSide = WFCHelper.GetOppositeSide(side);
                    _pattern[WFCHelper.PseudoTileId].Add(oppositeSide, tile);
                    return;
                }
                var neighbour = _placer.PlacedTiles[neighbourPos];

                var id = tile.TileId;
                if (!_pattern.ContainsKey(id))
                    _pattern.Add(id, new ProbableNeighbours());
                var localSide = WFCHelper.GetLocalSideByTurn(side, rotation);
                _pattern[id].Add(localSide, neighbour);
            }, _isEightDirectionAnalyze);
        }
    }
}
