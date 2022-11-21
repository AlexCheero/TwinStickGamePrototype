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

        public Cell(ICollection<PatternKey> tiles)
        {
            var baseChance = 1.0f / tiles.Count;
            AvailableTiles = new List<TileWithChance>();
            foreach (var tile in tiles)
                AvailableTiles.Add(new TileWithChance(tile, baseChance));
        }

        public void TryCollapse(Vector3 position)
        {
            var chance = 0.0f;
            TileWithChance selectedTileChance = default;
            foreach (var tileChance in AvailableTiles)
            {
                var shouldChooseTile = tileChance.Chance > chance;
                shouldChooseTile |= Mathf.Abs(tileChance.Chance - chance) < float.Epsilon && Random.value > 0.5f;
                if (!shouldChooseTile)
                    continue;
                chance = tileChance.Chance;
                selectedTileChance = tileChance;
            }

            if (chance == 0)
                return;

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

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                if (Grid == null)
                    InitGrid();
                
                var idx = GetLowestEntropyCellIdx();
                Debug.Log("idx: " + idx);
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
                if (Grid == null)
                    InitGrid();
                
                _placer.Clear();
                ClearGrid();
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
        
        public void GenerateStep(int idx)
        {
            var halfDim = Dim / 2;
            var x = idx % Dim;
            var y = idx / Dim;
            var position = new Vector3(x - halfDim, 0, y - halfDim);
            Grid[idx].TryCollapse(position);
            if (Grid[idx].IsCollapsed)
            {
                var key = Grid[idx].PKey;
                _placer.PlaceTile(key.Id, position, key.YRotation);
                // for (int j = y - 1; j < y + 2; j++)
                // {
                //     for (int i = x - 1; i < x + 2; i++)
                //     {
                //         
                //     }
                // }

                var tileNeighbours = _analyzer.Pattern[key].Neighbours;
                
                if (tileNeighbours.ContainsKey(ETileSide.Up))
                    RemoveAvailableTiles(x + (y + 1) * Dim, tileNeighbours[ETileSide.Up]);
                if (tileNeighbours.ContainsKey(ETileSide.Right))
                    RemoveAvailableTiles((x + 1) + y * Dim, tileNeighbours[ETileSide.Right]);
                if (tileNeighbours.ContainsKey(ETileSide.Down))
                    RemoveAvailableTiles(x + (y - 1) * Dim, tileNeighbours[ETileSide.Down]);
                if (tileNeighbours.ContainsKey(ETileSide.Left))
                    RemoveAvailableTiles((x - 1) + y * Dim, tileNeighbours[ETileSide.Left]);
            }
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
            
            //TODO: cache this set so it doesn't have to allocate every method call
            var deleteSet = new HashSet<int>();
            var availableTilesInNeighbour = Grid[idx].AvailableTiles;
                    
            for (int i = 0; i < availableTilesInNeighbour.Count; i++)
            {
                var isFound = false;
                foreach (var possibleNeighbour in possibleNeighbours)
                {
                    if (availableTilesInNeighbour[i].PKey.Id != possibleNeighbour.PKey.Id)
                        continue;
                    isFound = true;
                    var newChance = Mathf.Min(availableTilesInNeighbour[i].Chance, possibleNeighbour.Chance);
                    availableTilesInNeighbour[i] =
                        new Cell.TileWithChance(availableTilesInNeighbour[i].PKey, newChance);
                    break;
                }

                if (!isFound && !deleteSet.Add(i))
                    Debug.LogError("trying to add index for delete second time");
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
            
            var deleteList = deleteSet.ToList();
            deleteList.Sort((a, b) => b - a);
            foreach (var i in deleteList)
                availableTilesInNeighbour.RemoveAt(i);
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
    }
}