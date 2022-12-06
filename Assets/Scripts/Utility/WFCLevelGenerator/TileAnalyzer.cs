using System;
using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using WFC;

public class ProbableNeighbours
{
    //TODO: make private
    public Dictionary<ETileSide, List<Tuple<ETileSide, ProbableEntry>>> Neighbours;

    public ProbableNeighbours()
    {
        Neighbours = new Dictionary<ETileSide, List<Tuple<ETileSide, ProbableEntry>>>();
    }

    public void Add(ETileSide side, ETileSide adjacentSide, Tile tile)
    {
        if (!Neighbours.ContainsKey(side))
            Neighbours[side] = new List<Tuple<ETileSide, ProbableEntry>>();
        var neighboursOnSide = Neighbours[side];
        var tileId = neighboursOnSide.FindIndex(tuple => tuple.Item1 == adjacentSide && tuple.Item2.Id == tile.TileId);
        
        if (tileId < 0)
        {
            neighboursOnSide.Add(Tuple.Create(adjacentSide, new ProbableEntry(tile.TileId, 1)));
        }
        else
        {
            var neighbour = neighboursOnSide[tileId].Item2;
            neighbour.Weight++;
            //TODO: probably it is better to use custom mutable tuple not to allocate every time
            neighboursOnSide[tileId] = Tuple.Create(adjacentSide, neighbour);
        }
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
            var tileTransform = tile.transform;
            var pos = tileTransform.position;
            var rotation = tileTransform.GetYRotation();
            WFCHelper.ForEachSide((side, x, y) =>
            {
                var neighbourPos = pos;
                neighbourPos.x += x * _placer.SnapSize;
                neighbourPos.z += y * _placer.SnapSize;
                var oppositeSide = WFCHelper.GetOppositeSide(side);
                var localSide = WFCHelper.GetLocalSideByTurn(side, rotation);
                if (!_placer.PlacedTiles.ContainsKey(neighbourPos))
                {
                    if (!_pattern.ContainsKey(WFCHelper.PseudoTileId))
                        _pattern.Add(WFCHelper.PseudoTileId, new ProbableNeighbours());
                    _pattern[WFCHelper.PseudoTileId].Add(oppositeSide, localSide, tile);
                }
                else
                {
                    var id = tile.TileId;
                    if (!_pattern.ContainsKey(id))
                        _pattern.Add(id, new ProbableNeighbours());
                    var neighbour = _placer.PlacedTiles[neighbourPos];
                    var adjacentLocalSide = WFCHelper.GetLocalSideByTurn(oppositeSide, neighbour.transform.GetYRotation());
                    _pattern[id].Add(localSide, adjacentLocalSide, neighbour);
                }
            }, _isEightDirectionAnalyze);
        }
    }
}
