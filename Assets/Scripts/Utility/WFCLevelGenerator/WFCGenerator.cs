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

        public Cell(ICollection<PatternEntry> tiles)
        {
            var baseChance = 1.0f / tiles.Count;
            ProbableEntries = new List<ProbableEntry>();
            foreach (var tile in tiles)
                ProbableEntries.Add(new ProbableEntry(tile, baseChance));
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
    public class WFCGenerator : MonoBehaviour
    {
        public bool UseRandom;
        public int Dim;
        public Cell[] Grid;

        private TileAnalyzer _analyzer;
        private TilePlacer _placer;
        
        [SerializeField]
        private float _generateStepTime = 0.2f;
        private WaitForSeconds _generateDelay;
        private IEnumerator _generateCoroutine;
        
        void Awake()
        {
            _analyzer = GetComponent<TileAnalyzer>();
            _placer = GetComponent<TilePlacer>();
            _generateDelay = new WaitForSeconds(_generateStepTime);
        }

        IEnumerator Start()
        {
            while (_analyzer.Pattern == null)
                yield return null;
            InitGrid();
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
            }

            if (Input.GetKeyDown(KeyCode.C))
                Clear();

            if (Input.GetKeyDown(KeyCode.G))
            {
                Clear();
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    _generateCoroutine = GenerateCoroutine();
                    StartCoroutine(_generateCoroutine);
                }
                else
                {
                    Generate();
                }
            }
        }

        private void ClearGrid()
        {
            for (int i = 0; i < Grid.Length; i++)
                Grid[i] = new Cell(_analyzer.Pattern.Keys);
        }

        private void Clear()
        {
            StopGenerateCoroutine();
            _placer.Clear();
            ClearGrid();
        }

        private void InitGrid()
        {
            Grid = new Cell[Dim * Dim];
            for (int i = 0; i < Grid.Length; i++)
                Grid[i] = new Cell(_analyzer.Pattern.Keys);
        }

        private void GenerateStep(int idx)
        {
            var gridPos = IdxToGridPos(idx, Dim);
            Grid[idx].TryCollapse(UseRandom);
            if (Grid[idx].IsCollapsed)
                PlaceAndUpdateNeighbours(Grid[idx].Entry, gridPos);
        }

        private void PlaceAndUpdateNeighbours(PatternEntry pEntry, Vector2Int gridPosition)
        {
            var halfDim = Dim / 2;
            var position = new Vector3(gridPosition.x - halfDim, 0, gridPosition.y - halfDim);
            _placer.PlaceTile(pEntry.Id, position, pEntry.YRotation);
            
            var tileNeighbours = _analyzer.Pattern[pEntry].Neighbours;
            for (int j = 0; j < 3; j++)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (i == 1 && j == 1)
                        continue;
                    var side = (ETileSide)(i + j * 3);
                    if (!tileNeighbours.ContainsKey(side))
                        continue;
                    var neighbourGridPos = gridPosition;
                    neighbourGridPos.x += i - 1;
                    neighbourGridPos.y += j - 1;
                    RemoveUnavailableTiles(GridPosToIdx(neighbourGridPos, Dim), tileNeighbours[side]);
                }
            }
        }

        private void Generate()
        {
            var ctr = 0;
            var idx = GetLowestEntropyCellIdx();
            while (idx >= 0)
            {
                GenerateStep(idx);
                
                idx = GetLowestEntropyCellIdx();
                ctr++;

                if (ctr >= 10000)
                {
                    Debug.LogError("infinte loop");
                    break;
                }
            }
        }
        
        private IEnumerator GenerateCoroutine()
        {
            var ctr = 0;
            var idx = GetLowestEntropyCellIdx();
            while (idx >= 0)
            {
                GenerateStep(idx);

                yield return _generateDelay;
                
                idx = GetLowestEntropyCellIdx();
                ctr++;

                if (ctr >= 10000)
                {
                    Debug.LogError("infinte loop");
                    break;
                }
            }

            _generateCoroutine = null;
        }

        private void StopGenerateCoroutine()
        {
            if (_generateCoroutine != null)
            {
                StopCoroutine(_generateCoroutine);
                _generateCoroutine = null;
            }
        }

        private void RemoveUnavailableTiles(int idx, List<ProbableEntry> probableNeighbours)
        {
            if (idx < 0 || idx >= Grid.Length)
                return;

            var probableEntries = Grid[idx].ProbableEntries;
            if (probableNeighbours.Count == 0)
            {
                probableEntries.Clear();
                return;
            }

            for (int i = probableEntries.Count - 1; i >= 0; i--)
            {
                var probableNeighbourIdx =
                    probableNeighbours.FindIndex(neighbour =>
                    {
                        var isIdsEqual = neighbour.Entry.Id == probableEntries[i].Entry.Id;
                        var isRotationEqual =
                            Mathf.Abs(neighbour.Entry.YRotation - probableEntries[i].Entry.YRotation) < TileAnalyzer.RotationTolerance;
                        return isIdsEqual && isRotationEqual;
                    });
                
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
            for (int i = 0; i < Grid.Length; i++)
            {
                bool suitableEntropy = Grid[i].Entropy < lowestEntropy;
                suitableEntropy |= UseRandom && Grid[i].Entropy == lowestEntropy && Random.value > 0.5f;
                if (!Grid[i].IsCollapsed &&
                    Grid[i].ProbableEntries.Count > 0 &&
                    suitableEntropy)
                {
                    lowestEntropyTileIdx = i;
                    lowestEntropy = Grid[i].Entropy;
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

        private int GridPosToIdx(Vector2Int pos, int dim) => pos.x + pos.y * dim;
    }
}