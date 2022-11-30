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
        public float Chance;

        public ProbableEntry(PatternEntry entry, float chance)
        {
            Entry = entry;
            Chance = chance;
        }
    }
    
    public class Cell
    {
        public readonly List<ProbableEntry> ProbableEntries;
        public PatternEntry Entry { get; private set; }
        public bool IsCollapsed { get; private set; }

        public Cell(ICollection<PatternEntry> entries)
        {
            var baseChance = 1.0f / entries.Count;
            ProbableEntries = new List<ProbableEntry>();
            foreach (var entry in entries)
            {
                if (TileAnalyzer.EntryComparer.Equals(entry, PatternEntry.PseudoEntry))
                    continue;
                ProbableEntries.Add(new ProbableEntry(entry, baseChance));
            }
            IsCollapsed = false;
        }

        public void TryCollapse(bool useRandom)
        {
            var chance = 0.0f;
            ProbableEntry selectedEntry = default;
            foreach (var probableEntry in ProbableEntries)
            {
                var shouldChooseTile = probableEntry.Chance > chance;
                shouldChooseTile |= useRandom && Mathf.Abs(probableEntry.Chance - chance) < float.Epsilon && Random.value > 0.5f;
                if (!shouldChooseTile)
                    continue;
                chance = probableEntry.Chance;
                selectedEntry = probableEntry;
            }

            if (chance == 0)
            {
                IsCollapsed = false;
                return;
            }

            Entry = selectedEntry.Entry;
            IsCollapsed = true;
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
        private int _seed;
        [SerializeField]
        private int _dim;
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

            _fallBackTileIdx = Mathf.Clamp(_fallBackTileIdx, 0, _palette.Palette.Count - 1);
            _fallBackBorderTileIdx = Mathf.Clamp(_fallBackBorderTileIdx, 0, _palette.Palette.Count - 1);
        }

        IEnumerator Start()
        {
            while (_analyzer.Pattern == null)
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
                    Clear();
                    idx = GetLowestEntropyCellIdx();
                }

                GenerateStep(idx);
                PlaceTile(idx);
            }

            if (Input.GetKeyDown(KeyCode.C))
                Clear();

            if (Input.GetKeyDown(KeyCode.G))
            {
                Clear();
                var ctr = 0;
                while (!Generate() && _tryRegenerate)
                {
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

        private void Clear()
        {
            if (_useRandom && _setSeed)
                Random.InitState(_seed);
            _placer.Clear();
            InitGrid(false);
        }

        private bool IsBorderTile(int idx, int dim)
        {
            var pos = IdxToGridPos(idx, dim);
            return pos.x == 0 || pos.y == 0 || pos.x == dim - 1 || pos.y == dim - 1;
        }
        
        private void InitGrid(bool initNew)
        {
            if (initNew)
                _grid = new Cell[_dim * _dim];
            for (int i = 0; i < _grid.Length; i++)
            {
                if (IsBorderTile(i, _dim) && _analyzer.Pattern.ContainsKey(PatternEntry.PseudoEntry))
                {
                    var pseudoEntryNeighbours = _analyzer.Pattern[PatternEntry.PseudoEntry].Neighbours;
                    var patternEntries = _analyzer.Pattern.Keys.ToList();
                    var gridPos = IdxToGridPos(i, _dim);
                    WFCHelper.ForEachSide(_analyzer.IsEightDirectionAnalyze, (side, x, y) =>
                    {
                        var idx = i;
                        var neighborX = gridPos.x + x;
                        var neighborY = gridPos.y + y;
                        if (neighborX < 0 || neighborX >= _dim || neighborY < 0 || neighborY >= _dim)
                        {
                            var oppositeSide = (ETileSide)(8 - (int)side);
                            var probableEntries = pseudoEntryNeighbours[oppositeSide];
                            for (int j = patternEntries.Count - 1; j >= 0; j--)
                            {
                                var probableNeighbourIdx =
                                    probableEntries.FindIndex(probableEntry =>
                                        TileAnalyzer.EntryComparer.Equals(probableEntry.Entry, patternEntries[j]));
                                if (probableNeighbourIdx < 0)
                                    patternEntries.RemoveAt(j);
                            }
                        }
                    });
                    if (patternEntries.Count == 0 || (patternEntries.Count == 1 && patternEntries[0].Id == -1))
                        Debug.LogError("empty patternEntries for cell " + i + ". on grid init");
                    _grid[i] = new Cell(patternEntries);
                }
                else
                {
                    _grid[i] = new Cell(_analyzer.Pattern.Keys);
                }
            }
        }

        private void GenerateStep(int idx)
        {
            var gridPos = IdxToGridPos(idx, _dim);
            _grid[idx].TryCollapse(_useRandom);
            if (_grid[idx].IsCollapsed)
                UpdateNeighbours(_grid[idx].Entry, gridPos);
        }

        private void PlaceTile(int idx)
        {
            var cell = _grid[idx];
            if (!cell.IsCollapsed && (!_useFallbackTile || cell.ProbableEntries.Count > 0))
                return;
            var pEntry = cell.Entry;
            var gridPos = IdxToGridPos(idx, _dim);
            if (_useFallbackTile && cell.ProbableEntries.Count == 0)
            {
                if (IsBorderTile(idx, _dim))
                {
                    pEntry.Id = _fallBackBorderTileIdx;
                    if (gridPos.x == 0)
                        pEntry.YRotation = 90;
                    else if (gridPos.x == _dim - 1)
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
            }
#if DEBUG
            if (idx < 0 || idx >= _grid.Length)
                throw new Exception("wrong idx");
#endif
            var halfDim = _dim / 2;
            var position = new Vector3(gridPos.x - halfDim, 0, gridPos.y - halfDim);
            
            _placer.PlaceTile(pEntry.Id, position, pEntry.YRotation);
        }
        
        private void UpdateNeighbours(PatternEntry pEntry, Vector2Int gridPosition)
        {
            var tileNeighbours = _analyzer.Pattern[pEntry].Neighbours;
            WFCHelper.ForEachSide(_analyzer.IsEightDirectionAnalyze, (side, x, y) =>
            {
                if (!tileNeighbours.ContainsKey(side))
                    return;
                var neighbourGridPos = gridPosition;
                neighbourGridPos.x += x;
                neighbourGridPos.y += y;
                RemoveUnavailableTiles(GridPosToIdx(neighbourGridPos, _dim), tileNeighbours[side]);
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
                    probableNeighbours.FindIndex(neighbour =>
                        TileAnalyzer.EntryComparer.Equals(neighbour.Entry, probableEntries[i].Entry));
                if (probableNeighbourIdx < 0)
                    probableEntries.RemoveAt(i);
            }
            
#region NormalizingTileChance

            var overallChance = probableEntries.Sum(tileWithChance => tileWithChance.Chance);
#if DEBUG
            if (overallChance > 1)
                Debug.LogError("chance is bigger than 1, after removing unavailable tiles");
#endif
            for (int i = 0; i < probableEntries.Count; i++)
            {
                var normalizedChance = probableEntries[i].Chance / overallChance;
                probableEntries[i] =
                    new ProbableEntry(probableEntries[i].Entry, normalizedChance);
            }
            
#if DEBUG
            overallChance = probableEntries.Sum(tileWithChance => tileWithChance.Chance);
            const float chanceTolerance = 0.0001f;
            if (probableEntries.Count > 0 && Mathf.Abs(overallChance - 1) > chanceTolerance)
                Debug.LogError("overall chance ("+ overallChance + ") is not equal to 1 after normalization");
#endif

#endregion
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

        private Vector2Int IdxToGridPos(int idx, int dim) =>
            new Vector2Int
            {
                x = idx % dim,
                y = idx / dim
            };

        private int GridPosToIdx(Vector2Int pos, int dim)
        {
            if (pos.x < 0 || pos.y < 0 || pos.x >= dim || pos.y >= dim)
                return -1;
            return pos.x + pos.y * dim;
        }
    }
}