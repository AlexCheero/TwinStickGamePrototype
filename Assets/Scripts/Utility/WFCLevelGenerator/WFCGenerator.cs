using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace WFC
{
    public class Cell
    {
        public struct TileWithChance
        {
            public PatternKey PKey;
            public float Chance;

            public TileWithChance(PatternKey pKey, float chance)
            {
                PKey = pKey;
                Chance = chance;
            }
        }
        //tuple for tile and its chance
        public List<TileWithChance> AvailableTiles;
        public PatternKey PKey { get; private set; }
        public bool IsCollapsed { get; private set; }

        public void SetManually(PatternKey pKey)
        {
            PKey = pKey;
            IsCollapsed = true;
        }

        public Cell(ICollection<PatternKey> tiles)
        {
            var baseChance = 1.0f / tiles.Count;
            AvailableTiles = new List<TileWithChance>();
            foreach (var tile in tiles)
                AvailableTiles.Add(new TileWithChance(tile, baseChance));
            IsCollapsed = false;
        }

        public void TryCollapse()
        {
            var chance = 0.0f;
            TileWithChance selectedTileChance = default;
            foreach (var tileChance in AvailableTiles)
            {
                var shouldChooseTile = tileChance.Chance > chance;
                //shouldChooseTile |= Mathf.Abs(tileChance.Chance - chance) < float.Epsilon && Random.value > 0.5f;
                if (!shouldChooseTile)
                    continue;
                chance = tileChance.Chance;
                selectedTileChance = tileChance;
            }

            if (chance == 0)
            {
                IsCollapsed = false;
                return;
            }

            PKey = selectedTileChance.PKey;
            IsCollapsed = true;
        }

        public int Entropy => AvailableTiles.Count;
    }

    [RequireComponent(typeof(TileAnalyzer))]
    [RequireComponent(typeof(TilePlacer))]
    public class WFCGenerator : MonoBehaviour
    {
        public int Dim;
        public Cell[] Grid;

        private TileAnalyzer _analyzer;
        private TilePlacer _placer;
        
        void Awake()
        {
            _analyzer = GetComponent<TileAnalyzer>();
            _placer = GetComponent<TilePlacer>();
        }

        IEnumerator Start()
        {
            while (_analyzer.Pattern == null)
                yield return null;
            InitGrid();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
                PrepareOuterWalls();
            
            if (Input.GetKeyDown(KeyCode.S))
            {
                var idx = GetLowestEntropyCellIdx();
                if (idx < 0)
                {
                    _placer.Clear();
                    ClearGrid();
                    idx = GetLowestEntropyCellIdx();
                }

                GenerateStep(idx);
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                _placer.Clear();
                ClearGrid();
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                _placer.Clear();
                ClearGrid();
                PrepareOuterWalls();
                Generate();
            }
        }

        private void ClearGrid()
        {
            for (int i = 0; i < Grid.Length; i++)
                Grid[i] = new Cell(_analyzer.Pattern.Keys);
        }

        private void InitGrid()
        {
            Grid = new Cell[Dim * Dim];
            for (int i = 0; i < Grid.Length; i++)
                Grid[i] = new Cell(_analyzer.Pattern.Keys);
        }

        private void PrepareOuterWalls()
        {
            for (int i = 0; i < Grid.Length; i++)
            {
                var gridPos = IdxToGridPos(i, Dim);
                if (gridPos.x == 0 && gridPos.y == 0)
                    Grid[i].SetManually(new PatternKey { Id = 0, name = "AngleWallTile(Clone)", YRotation = 0 });
                else if (gridPos.x == Dim - 1 && gridPos.y == 0)
                    Grid[i].SetManually(new PatternKey { Id = 0, name = "AngleWallTile(Clone)", YRotation = 270 });
                else if (gridPos.x == 0 && gridPos.y == Dim - 1)
                    Grid[i].SetManually(new PatternKey { Id = 0, name = "AngleWallTile(Clone)", YRotation = 90 });
                else if (gridPos.x == Dim - 1 && gridPos.y == Dim - 1)
                    Grid[i].SetManually(new PatternKey { Id = 0, name = "AngleWallTile(Clone)", YRotation = 180 });
                else if (gridPos.x == 0)
                {
                    if (Random.value > 0.5f)
                        Grid[i].SetManually(new PatternKey { Id = 1, name = "WallTile(Clone)", YRotation = 90 });
                    else
                        Grid[i].SetManually(new PatternKey { Id = 0, name = "AngleWallTile(Clone)", YRotation = 0 });
                }
                else if (gridPos.x == Dim - 1)
                {
                    if (Random.value > 0.5f)
                        Grid[i].SetManually(new PatternKey { Id = 1, name = "WallTile(Clone)", YRotation = 270 });
                    else
                        Grid[i].SetManually(new PatternKey { Id = 0, name = "AngleWallTile(Clone)", YRotation = 270 });
                }
                else if (gridPos.y == 0)
                {
                    if (Random.value > 0.5f)
                        Grid[i].SetManually(new PatternKey { Id = 1, name = "WallTile(Clone)", YRotation = 0 });
                    else
                        Grid[i].SetManually(new PatternKey { Id = 0, name = "AngleWallTile(Clone)", YRotation = 0 });
                }
                else if (gridPos.y == Dim - 1)
                {
                    if (Random.value > 0.5f)
                        Grid[i].SetManually(new PatternKey { Id = 1, name = "WallTile(Clone)", YRotation = 180 });
                    else
                        Grid[i].SetManually(new PatternKey { Id = 0, name = "AngleWallTile(Clone)", YRotation = 180 });
                }
                
                if (Grid[i].IsCollapsed)
                    PlaceAndUpdateNeighbours(Grid[i].PKey, gridPos);
            }
        }
        
        private void GenerateStep(int idx)
        {
            var gridPos = IdxToGridPos(idx, Dim);
            Grid[idx].TryCollapse();
            if (Grid[idx].IsCollapsed)
                PlaceAndUpdateNeighbours(Grid[idx].PKey, gridPos);
        }

        private void PlaceAndUpdateNeighbours(PatternKey pKey, Vector2Int gridPosition)
        {
            var halfDim = Dim / 2;
            var position = new Vector3(gridPosition.x - halfDim, 0, gridPosition.y - halfDim);
            _placer.PlaceTile(pKey.Id, position, pKey.YRotation);
            // for (int j = y - 1; j < y + 2; j++)
            // {
            //     for (int i = x - 1; i < x + 2; i++)
            //     {
            //         
            //     }
            // }
            
            var tileNeighbours = _analyzer.Pattern[pKey].Neighbours;
                
            if (tileNeighbours.ContainsKey(ETileSide.Up))
                RemoveAvailableTiles(gridPosition.x + (gridPosition.y + 1) * Dim, tileNeighbours[ETileSide.Up]);
            if (tileNeighbours.ContainsKey(ETileSide.Right))
                RemoveAvailableTiles((gridPosition.x + 1) + gridPosition.y * Dim, tileNeighbours[ETileSide.Right]);
            if (tileNeighbours.ContainsKey(ETileSide.Down))
                RemoveAvailableTiles(gridPosition.x + (gridPosition.y - 1) * Dim, tileNeighbours[ETileSide.Down]);
            if (tileNeighbours.ContainsKey(ETileSide.Left))
                RemoveAvailableTiles((gridPosition.x - 1) + gridPosition.y * Dim, tileNeighbours[ETileSide.Left]);
        }
        
        public void Generate()
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

        void RemoveAvailableTiles(int idx, List<PossibleNeighbour> possibleNeighbours)
        {
            if (idx < 0 || idx >= Grid.Length)
                return;

            var availableTilesInNeighbour = Grid[idx].AvailableTiles;
            if (possibleNeighbours.Count == 0)
            {
                availableTilesInNeighbour.Clear();
                return;
            }

            for (int i = availableTilesInNeighbour.Count - 1; i >= 0; i--)
            {
                var possibleNeighbourIdx =
                    possibleNeighbours.FindIndex(neighbour =>
                    {
                        var isIdsEqual = neighbour.PKey.Id == availableTilesInNeighbour[i].PKey.Id;
                        const float rotationTolerance = 0.001f;
                        var isRotationEqual =
                            Mathf.Abs(neighbour.PKey.YRotation - availableTilesInNeighbour[i].PKey.YRotation) < rotationTolerance;
                        return isIdsEqual && isRotationEqual;
                    });
                
                if (possibleNeighbourIdx < 0)
                    availableTilesInNeighbour.RemoveAt(i);
            }
            
#region NormalizingTileChance

            var overallChance = availableTilesInNeighbour.Sum(tileWithChance => tileWithChance.Chance);
#if DEBUG
            if (overallChance > 1)
                Debug.LogError("chance is bigger than 1, after removing available tiles");
#endif
            for (int i = 0; i < availableTilesInNeighbour.Count; i++)
            {
                var normalizedChance = availableTilesInNeighbour[i].Chance / overallChance;
                availableTilesInNeighbour[i] =
                    new Cell.TileWithChance(availableTilesInNeighbour[i].PKey, normalizedChance);
            }
            
#if DEBUG
            overallChance = availableTilesInNeighbour.Sum(tileWithChance => tileWithChance.Chance);
            const float chanceTolerance = 0.0001f;
            if (availableTilesInNeighbour.Count > 0 && Mathf.Abs(overallChance - 1) > chanceTolerance)
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
                //suitableEntropy |= Grid[i].Entropy == lowestEntropy && Random.value > 0.5f;
                if (!Grid[i].IsCollapsed &&
                    Grid[i].AvailableTiles.Count > 0 &&
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