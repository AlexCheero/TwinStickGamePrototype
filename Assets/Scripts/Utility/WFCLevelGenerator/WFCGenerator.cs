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
        public PatternEntry Entry;
#if DEBUG
        private float _weight;
        public float Weight
        {
            get => _weight;
            set
            {
                if (value <= 0)
                    throw new Exception("Tile weight could not be less or equal to zero");
                _weight = value;
            }
        }
#else
        public float Weight;
#endif

        public ProbableEntry(PatternEntry entry, float weight)
        {
            Entry = entry;
#if DEBUG
            _weight = weight;
#else
            Weight = weight;
#endif
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

        public PatternEntry Entry => ProbableEntries.Count > 0 ? ProbableEntries[0].Entry : new PatternEntry();

        public bool IsCollapsed => _collapseState == ECollapseState.Collapsed ||
                                   _collapseState == ECollapseState.CollapsedManually;

        public bool IsCollapsedManually => _collapseState == ECollapseState.CollapsedManually;
        
        public Cell(IEnumerable<PatternEntry> entries)
        {
            ProbableEntries = new List<ProbableEntry>();
            foreach (var entry in entries)
            {
                if (TileAnalyzer.EntryComparer.Equals(entry, PatternEntry.PseudoEntry))
                    continue;
                ProbableEntries.Add(new ProbableEntry(entry, 1));
            }
            _collapseState = ECollapseState.NotCollapsed;
        }

        public bool TryCollapse(bool useRandom)
        {
            if (ProbableEntries.Count == 0)
            {
                _collapseState = ECollapseState.NotCollapsed;
                return false;
            }
            
            var chance = 0.0f;
            ProbableEntry selectedEntry = default;
            foreach (var probableEntry in ProbableEntries)
            {
                var newChance = Random.value * probableEntry.Weight;
                if (newChance > chance || Mathf.Abs(chance - newChance) < float.Epsilon && Random.value > 0.5f)
                {
                    chance = Mathf.Max(newChance, chance);
                    selectedEntry = probableEntry;
                }
            }

            ProbableEntries.Clear();
            ProbableEntries.Add(selectedEntry);
            _collapseState = ECollapseState.Collapsed;
            return true;
        }

        public void CollapseManually(int id, float rotation)
        {
            ProbableEntries.Clear();
            ProbableEntries.Add(new ProbableEntry(new PatternEntry { Id = id, YRotation = rotation }, 1));
            _collapseState = ECollapseState.CollapsedManually;
        }

        //public int Entropy => ProbableEntries.Count;
        
        //TODO: recheck the Shannon entropy formula
        public float GetEntropy()
        {
            var totalWeight = ProbableEntries.Sum(entry => entry.Weight);
            var entropy = 0.0f;
            for (int i = 0; i < ProbableEntries.Count; i++)
            {
                var weight = ProbableEntries[i].Weight;
                if (weight > 0)
                {
                    var p = weight / totalWeight;
                    entropy -= p * Mathf.Log(p) / Mathf.Log(2);
                }
            }
                
            return entropy;
        }
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
        [SerializeField]
        private bool _isEightDirectionWave = true;

        private Cell[] _grid;
        private Queue<int> _updateQueue;

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
            
            _updateQueue = new Queue<int>();
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
                
                if (IsBorderTile(i, dimension) && _analyzer.Contains(PatternEntry.PseudoEntry))
                {
                    var pseudoEntryNeighbours = _analyzer[PatternEntry.PseudoEntry].Neighbours;
                    var patternEntries = _analyzer.Keys.ToList();
                    var gridPos = WFCHelper.IdxToGridPos(i, dimension);
                    for (int j = 0; j < 8; j++)
                    {
                        if (!_isEightDirectionWave && j % 2 != 0)
                            continue;
                        
                        var bias = WFCHelper.BiasPerSide[(ETileSide)j];
                        var neighbourPos = gridPos + bias;
                        if (neighbourPos.x < 0 || neighbourPos.x >= dimension || neighbourPos.y < 0 || neighbourPos.y >= dimension)
                        {
                            var oppositeSide = WFCHelper.GetOppositeSide((ETileSide)j);
                            if (!pseudoEntryNeighbours.ContainsKey(oppositeSide))
                                continue;
                            var probableEntries = pseudoEntryNeighbours[oppositeSide];
                            for (int k = patternEntries.Count - 1; k >= 0; k--)
                            {
                                var probableNeighbourIdx =
                                    probableEntries.FindIndex(probableEntry =>
                                        TileAnalyzer.EntryComparer.Equals(probableEntry.Entry, patternEntries[k]));
                                if (probableNeighbourIdx < 0)
                                    patternEntries.RemoveAt(k);
                            }
                        }
                    }
                    if (patternEntries.Count == 0 || (patternEntries.Count == 1 && patternEntries[0].Id == -1))
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
                UpdateNeighbours(gridPos);
            }
        }

        private void GenerateStep(int idx)
        {
            if (!_grid[idx].TryCollapse(_useRandom))
                return;
            
            var gridPos = WFCHelper.IdxToGridPos(idx, _placer.Dimension);
            UpdateNeighbours(gridPos);
        }

        private void UpdateNeighbours(Vector2Int gridPos)
        {
            AddNeighboursToUpdateQueue(gridPos);
            var ctr = 0;
            while (_updateQueue.Count > 0 && ctr++ < 500)
            {
                var queuedIdx = _updateQueue.Dequeue();
                if (UpdateCell(queuedIdx))
                    AddNeighboursToUpdateQueue(WFCHelper.IdxToGridPos(queuedIdx, _placer.Dimension));
            }
        }
        
        private void AddNeighboursToUpdateQueue(Vector2Int gridPos)
        {
            for (int i = 0; i < 8; i++)
            {
                if (!_isEightDirectionWave && i % 2 != 0)
                    continue;
                var bias = WFCHelper.BiasPerSide[(ETileSide)i];
                var neighbourPos = gridPos + bias;
                var neighbourIdx = WFCHelper.GridPosToIdx(neighbourPos, _placer.Dimension);
                if (neighbourIdx > 0 && !_updateQueue.Contains(neighbourIdx))
                    _updateQueue.Enqueue(neighbourIdx);
            }
        }

        private bool UpdateCell(int idx)
        {
            var cell = _grid[idx];
            if (cell.IsCollapsed || cell.ProbableEntries.Count == 0)
                return false;
            var pos = WFCHelper.IdxToGridPos(idx, _placer.Dimension);
            var probableEntries = cell.ProbableEntries;
            bool removed = false;
            for (int i = probableEntries.Count - 1; i >= 0; i--)
            {
                var newWeight = GetNewWeight(probableEntries[i], pos);
                if (newWeight <= 0)
                {
                    probableEntries.RemoveAt(i);
                    removed = true;
                }
                else
                {
                    var entry = probableEntries[i];
                    entry.Weight = Mathf.Min(entry.Weight, newWeight);
                    probableEntries[i] = entry;
                }
            }
            
            return removed;
        }

        private void PlaceTile(int idx)
        {
            var cell = _grid[idx];
            bool shouldPlace = cell.IsCollapsed || _useFallbackTile;
            if (!shouldPlace)
                return;
            
            var dimension = _placer.Dimension;
            var pEntry = cell.Entry;
            var gridPos = WFCHelper.IdxToGridPos(idx, dimension);
            if (!cell.IsCollapsed)
            {
                if (IsBorderTile(idx, dimension))
                {
                    pEntry.Id = _fallBackBorderTileIdx;
                    if (gridPos.x == 0)
                        pEntry.YRotation = 90;
                    else if (gridPos.x == dimension - 1)
                        pEntry.YRotation = 270;
                    else if (gridPos.y == 0)
                        pEntry.YRotation = 0;
                    else
                        pEntry.YRotation = 180;
                }
                else
                {
                    pEntry.Id = _fallBackTileIdx;
                    pEntry.YRotation = 0;
                }

#if DEBUG
                _grid[idx].FallbackUsed = true;
#endif
            }
#if DEBUG
            if (idx < 0 || idx >= _grid.Length)
                throw new Exception("wrong idx");
#endif
            _placer.PlaceTile(pEntry.Id, WFCHelper.GridPosToPos(gridPos, dimension), pEntry.YRotation);
        }

        private void OnTilePlacedManually(int tileId, Vector3 position, float yRotation)
        {
            var gridPos = WFCHelper.PosToGridPos(position, _placer.Dimension);
            var idx = WFCHelper.GridPosToIdx(gridPos, _placer.Dimension);
            _grid[idx].CollapseManually(tileId, yRotation);
            UpdateNeighbours(gridPos);
        }
        
        private float GetNewWeight(ProbableEntry pEntry, Vector2Int gridPos)
        {
            for (int i = 0; i < 8; i++)
            {
                if (!_isEightDirectionWave && i % 2 != 0)
                    continue;
                var side = (ETileSide)i;
                var bias = WFCHelper.BiasPerSide[side];
                var neighbourIdx = WFCHelper.GridPosToIdx(gridPos + bias, _placer.Dimension);
                if (neighbourIdx < 0 || neighbourIdx >= _grid.Length)
                    continue;
                var probableEntries = _grid[neighbourIdx].ProbableEntries;
                var adjacentSide = WFCHelper.GetOppositeSide(side);
                var availableAtSide = false;
                for (int j = 0; j < probableEntries.Count; j++)
                {
                    var probableNeighbours = _analyzer[probableEntries[j].Entry].Neighbours;
                    if (!probableNeighbours.ContainsKey(adjacentSide))
                        continue;
                    var possibleTiles = probableNeighbours[adjacentSide];
                    var foundIdx = possibleTiles.FindIndex(p => TileAnalyzer.EntryComparer.Equals(p.Entry, pEntry.Entry));
                    if (foundIdx < 0)
                        continue;
                    pEntry.Weight = Mathf.Min(pEntry.Weight, possibleTiles[foundIdx].Weight);
                    availableAtSide = true;
                    break;
                }

                if (!availableAtSide)
                    return -1;
            }

            return pEntry.Weight;
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

        private int GetLowestEntropyCellIdx()
        {
            int lowestEntropyTileIdx = -1;
            float lowestEntropy = float.MaxValue;
            for (int i = 0; i < _grid.Length; i++)
            {
                var entropy = _grid[i].GetEntropy();
                bool suitableEntropy = entropy < lowestEntropy;
                suitableEntropy |= _useRandom && Mathf.Abs(entropy - lowestEntropy) < float.Epsilon && Random.value > 0.5f;
                if (!_grid[i].IsCollapsed &&
                    _grid[i].ProbableEntries.Count > 0 &&
                    suitableEntropy)
                {
                    lowestEntropyTileIdx = i;
                    lowestEntropy = entropy;
                }
            }

            return lowestEntropyTileIdx;
        }
    }
}