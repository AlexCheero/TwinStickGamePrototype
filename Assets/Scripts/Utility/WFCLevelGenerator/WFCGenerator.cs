using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace WFC
{
    public struct ProbableEntry
    {
        public int Id;
        public float Weight;

        public ProbableEntry(int id, float weight)
        {
            Id = id;
            Weight = weight;
        }
    }
    
    public class Cell
    {
        private enum ECollapseState
        {
            NotCollapsed,
            Collapsed,
            CollapsedManually
        }
        
        public readonly List<ProbableEntry> ProbableEntries;
        
        private ECollapseState _collapseState;
        
#if DEBUG
        public bool FallbackUsed;
#endif
        
        public int Id;
        public float Rotation;

        public bool IsCollapsed => _collapseState == ECollapseState.Collapsed ||
                                   _collapseState == ECollapseState.CollapsedManually;

        public bool IsCollapsedManually => _collapseState == ECollapseState.CollapsedManually;
        
        public Cell(IEnumerable<int> ids)
        {
            ProbableEntries = new List<ProbableEntry>();
            foreach (var id in ids)
            {
                if (id == WFCHelper.PseudoTileId)
                    continue;
                ProbableEntries.Add(new ProbableEntry(id, 1));
            }
            _collapseState = ECollapseState.NotCollapsed;
        }

        public void TryCollapse(bool useRandom)
        {
            var weight = 0.0f;
            ProbableEntry selectedEntry = default;
            foreach (var probableEntry in ProbableEntries)
            {
                var shouldChooseTile = probableEntry.Weight > weight;
                shouldChooseTile |= useRandom && Mathf.Abs(probableEntry.Weight - weight) < float.Epsilon && Random.value > 0.5f;
                if (!shouldChooseTile)
                    continue;
                weight = probableEntry.Weight;
                selectedEntry = probableEntry;
            }

            if (weight == 0)
            {
                _collapseState = ECollapseState.NotCollapsed;
                return;
            }

            Id = selectedEntry.Id;
            _collapseState = ECollapseState.Collapsed;
        }

        public void CollapseManually(int id, float rotation)
        {
            Id = id;
            Rotation = rotation;
            _collapseState = ECollapseState.CollapsedManually;
        }

        public int Entropy => ProbableEntries.Count;
    }

    [RequireComponent(typeof(TileAnalyzer))]
    [RequireComponent(typeof(TilePlacer))]
    [RequireComponent(typeof(TilePalette))]
    public class WFCGenerator : MonoBehaviour
    {
        [SerializeField]
        private bool _useRandom;
        [SerializeField]
        private bool _setSeed;
        [SerializeField]
        private bool _incrementSeed;
        [SerializeField]
        private int _seed;
        [SerializeField]
        private bool _tryRegenerate;
        [SerializeField]
        private int _regenAttempts = 1000;
        [SerializeField]
        private bool _useFallbackTile;
        [SerializeField]
        private int _fallBackTileIdx;
        [SerializeField]
        private int _fallBackBorderTileIdx;

        private Cell[] _grid;

        private TileAnalyzer _analyzer;
        private TilePlacer _placer;
        private TilePalette _palette;
        
        void Awake()
        {
            _analyzer = GetComponent<TileAnalyzer>();
            _placer = GetComponent<TilePlacer>();
            _palette = GetComponent<TilePalette>();

            _placer.OnPlaced += OnTilePlacedManually;
            _fallBackTileIdx = Mathf.Clamp(_fallBackTileIdx, 0, _palette.Palette.Count - 1);
            _fallBackBorderTileIdx = Mathf.Clamp(_fallBackBorderTileIdx, 0, _palette.Palette.Count - 1);
        }

        void OnDestroy()
        {
            _placer.OnPlaced -= OnTilePlacedManually;
        }

        IEnumerator Start()
        {
            if (_useRandom && _setSeed)
                Random.InitState(_seed);
            while (!_analyzer.IsPatternInited)
                yield return null;
            InitGrid(true);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                var idx = GetLowestEntropyCellIdx();
                if (idx < 0)
                {
                    Clear(false);
                    idx = GetLowestEntropyCellIdx();
                }

                GenerateStep(idx);
                PlaceTile(idx);
            }

            if (Input.GetKeyDown(KeyCode.C))
                Clear(true);

            if (Input.GetKeyDown(KeyCode.G))
            {
                Clear(false);
                var ctr = 0;
                while (!Generate() && _tryRegenerate)
                {
                    InitGrid(false);
                    ctr++;
                    if (ctr >= _regenAttempts)
                    {
                        Debug.LogWarning("collapse attempts exceeded");
                        break;
                    }
                }
                Debug.Log("collapsed in " + ctr + " attempts");

                for (int i = 0; i < _grid.Length; i++)
                    PlaceTile(i);
            }
        }

        private void Clear(bool clearManuallyCollapsed)
        {
            if (_useRandom && _setSeed)
            {
                Random.InitState(_seed);
                if (_incrementSeed)
                    _seed++;
            }
            _placer.Clear();
            InitGrid(clearManuallyCollapsed);
        }

        private bool IsBorderTile(int idx, int dim)
        {
            var pos = WFCHelper.IdxToGridPos(idx, dim);
            return pos.x == 0 || pos.y == 0 || pos.x == dim - 1 || pos.y == dim - 1;
        }
        
        private void InitGrid(bool clearManuallyCollapsed)
        {
            var dimension = _placer.Dimension;
            _grid ??= new Cell[dimension * dimension];
            for (int i = 0; i < _grid.Length; i++)
            {
                if (!clearManuallyCollapsed && _grid[i] != null && _grid[i].IsCollapsedManually)
                    continue;
                
                if (IsBorderTile(i, dimension) && _analyzer.Contains(WFCHelper.PseudoTileId))
                {
                    var pseudoEntryNeighbours = _analyzer[WFCHelper.PseudoTileId].Neighbours;
                    var patternEntries = _analyzer.Keys.ToList();
                    var gridPos = WFCHelper.IdxToGridPos(i, dimension);
                    WFCHelper.ForEachSide((side, x, y) =>
                    {
                        var neighborX = gridPos.x + x;
                        var neighborY = gridPos.y + y;
                        if (neighborX < 0 || neighborX >= dimension || neighborY < 0 || neighborY >= dimension)
                        {
                            var oppositeSide = WFCHelper.GetOppositeSide(side);
                            if (!pseudoEntryNeighbours.ContainsKey(oppositeSide))
                                return;
                            var probableEntries = pseudoEntryNeighbours[oppositeSide];
                            for (int j = patternEntries.Count - 1; j >= 0; j--)
                            {
                                var probableNeighbourIdx =
                                    probableEntries.FindIndex(probableEntry => probableEntry.Id == patternEntries[j]);
                                if (probableNeighbourIdx < 0)
                                    patternEntries.RemoveAt(j);
                            }
                        }
                    });
                    if (patternEntries.Count == 0 || (patternEntries.Count == 1 && patternEntries[0] == -1))
                        Debug.LogError("empty patternEntries for cell " + i + ". on grid init");
                    _grid[i] = new Cell(patternEntries);
                }
                else
                {
                    _grid[i] = new Cell(_analyzer.Keys);
                }
            }

            for (int i = 0; i < _grid.Length; i++)
            {
                if (!_grid[i].IsCollapsedManually)
                    continue;
                var gridPos = WFCHelper.IdxToGridPos(i, _placer.Dimension);
                UpdateNeighbours(_grid[i].Id, gridPos);
            }
        }

        private void GenerateStep(int idx)
        {
            var gridPos = WFCHelper.IdxToGridPos(idx, _placer.Dimension);
            _grid[idx].TryCollapse(_useRandom);
            if (_grid[idx].IsCollapsed)
                UpdateNeighbours(_grid[idx].Id, gridPos);
        }

        private void PlaceTile(int idx)
        {
            var cell = _grid[idx];
            bool shouldPlace = cell.IsCollapsed || _useFallbackTile;
            if (!shouldPlace)
                return;
            
            var dimension = _placer.Dimension;
            var id = cell.Id;
            var rotation = cell.Rotation;
            var gridPos = WFCHelper.IdxToGridPos(idx, dimension);
            if (!cell.IsCollapsed)
            {
                if (IsBorderTile(idx, dimension))
                {
                    id = _fallBackBorderTileIdx;
                    if (gridPos.x == 0)
                        rotation = 90;
                    else if (gridPos.x == dimension - 1)
                        rotation = 270;
                    else if (gridPos.y == 0)
                        rotation = 0;
                    else
                        rotation = 180;
                }
                else
                {
                    id = _fallBackTileIdx;
                    rotation = 0;
                }

#if DEBUG
                _grid[idx].FallbackUsed = true;
#endif
            }
#if DEBUG
            if (idx < 0 || idx >= _grid.Length)
                throw new Exception("wrong idx");
#endif
            _placer.PlaceTile(id, WFCHelper.GridPosToPos(gridPos, dimension), rotation);
        }

        private void OnTilePlacedManually(int tileId, Vector3 position, float yRotation)
        {
            var gridPos = WFCHelper.PosToGridPos(position, _placer.Dimension);
            var idx = WFCHelper.GridPosToIdx(gridPos, _placer.Dimension);
            _grid[idx].CollapseManually(tileId, yRotation);
            UpdateNeighbours(_grid[idx].Id, gridPos);
        }
        
        private void UpdateNeighbours(int id, Vector2Int gridPosition)
        {
            var tileNeighbours = _analyzer[id].Neighbours;
            WFCHelper.ForEachSide((side, x, y) =>
            {
                if (!tileNeighbours.ContainsKey(side))
                    return;
                var neighbourGridPos = gridPosition;
                neighbourGridPos.x += x;
                neighbourGridPos.y += y;
                RemoveUnavailableTiles(WFCHelper.GridPosToIdx(neighbourGridPos, _placer.Dimension), tileNeighbours[side]);
            });
        }

        private bool Generate()
        {
            var notCollapsedCount = _grid.Length;
            var idx = GetLowestEntropyCellIdx();
            while (idx >= 0)
            {
                GenerateStep(idx);
                idx = GetLowestEntropyCellIdx();
                notCollapsedCount--;
            }

            return notCollapsedCount == 0;
        }
        
        private void RemoveUnavailableTiles(int idx, List<ProbableEntry> probableNeighbours)
        {
            if (idx < 0 || idx >= _grid.Length)
                return;

            var probableEntries = _grid[idx].ProbableEntries;
            if (probableNeighbours.Count == 0)
            {
                probableEntries.Clear();
                return;
            }

            for (int i = probableEntries.Count - 1; i >= 0; i--)
            {
                var probableNeighbourIdx =
                    probableNeighbours.FindIndex(neighbour => neighbour.Id == probableEntries[i].Id);
                if (probableNeighbourIdx < 0)
                    probableEntries.RemoveAt(i);
                else
                {
                    var entry = probableEntries[i];
                    entry.Weight = Mathf.Min(entry.Weight, probableNeighbours[probableNeighbourIdx].Weight);
                    probableEntries[i] = entry;
                }
            }
        }

        private int GetLowestEntropyCellIdx()
        {
            int lowestEntropyTileIdx = -1;
            int lowestEntropy = int.MaxValue;
            for (int i = 0; i < _grid.Length; i++)
            {
                bool suitableEntropy = _grid[i].Entropy < lowestEntropy;
                suitableEntropy |= _useRandom && _grid[i].Entropy == lowestEntropy && Random.value > 0.5f;
                if (!_grid[i].IsCollapsed &&
                    _grid[i].ProbableEntries.Count > 0 &&
                    suitableEntropy)
                {
                    lowestEntropyTileIdx = i;
                    lowestEntropy = _grid[i].Entropy;
                }
            }

            return lowestEntropyTileIdx;
        }
    }
}