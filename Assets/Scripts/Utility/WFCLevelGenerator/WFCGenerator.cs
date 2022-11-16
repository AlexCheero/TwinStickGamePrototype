using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace WFC
{
    public class Cell
    {
        public List<Tile> AvailableTiles;
        public Tile CollapsedTile;

        public Cell(List<Tile> tiles) => AvailableTiles = tiles.ToList();

        public void Collapse(Vector3 position)
        {
            var chance = 0.0f;
            Tile selectedTile = null;
            foreach (var tile in AvailableTiles)
            {
                var shouldChooseTile = tile.Chance > chance;
                shouldChooseTile |= tile.Chance == chance && Random.value > 0.5f;
                if (shouldChooseTile)
                {
                    chance = tile.Chance;
                    selectedTile = tile;
                }
            }

            if (selectedTile != null)
            {
                //CollapsedTile = GameObject.Instantiate(selectedTile, position, Quaternion.identity);
                CollapsedTile = GameObject.Instantiate(selectedTile);
                CollapsedTile.transform.position = position;
            }
        }

        public int Entropy => AvailableTiles.Count;
    }

    [RequireComponent(typeof(TilePalette))]
    [RequireComponent(typeof(TileAnalyzer))]
    public class WFCGenerator : MonoBehaviour
    {
        public int Dim;
        public Cell[] Grid;

        private TilePalette _palette;
        private TileAnalyzer _analyzer;
        
        private Stack<int> _history;

        void Awake()
        {
            _palette = GetComponent<TilePalette>();
            _analyzer = GetComponent<TileAnalyzer>();
            Grid = new Cell[Dim * Dim];
            for (int i = 0; i < Grid.Length; i++)
                Grid[i] = new Cell(_palette.Palette);

            _history = new Stack<int>(Grid.Length);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                var idx = GetLowestEntropyCellIdx();
                Debug.Log("idx: " + idx);
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
                Generate();
            }
            
            if (Input.GetKeyDown(KeyCode.U))
            {
                Undo();
            }
        }

        private void Clear()
        {
            _history.Clear();
            
            for (var i = 0; i < Grid.Length; i++)
            {
                var tile = Grid[i].CollapsedTile;
                if (tile != null)
                    Destroy(tile.gameObject);
                Grid[i] = new Cell(_palette.Palette);
            }
        }

        public void GenerateStep(int idx)
        {
            var halfDim = Dim / 2;
            var x = idx % Dim;
            var y = idx / Dim;
            var position = new Vector3(x - halfDim, 0, y - halfDim);
            Grid[idx].Collapse(position);
            _history.Push(idx);
            var tile = Grid[idx].CollapsedTile;
            if (tile != null)
            {
                // for (int j = y - 1; j < y + 2; j++)
                // {
                //     for (int i = x - 1; i < x + 2; i++)
                //     {
                //         
                //     }
                // }

                var tileNeighbours = _analyzer.Pattern[tile.TileId].Neighbours;
                
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
            
            var deleteSet = new HashSet<int>();
            var possibleTilesInNeighbour = Grid[idx].AvailableTiles;
                    
            for (int i = 0; i < possibleTilesInNeighbour.Count; i++)
            {
                var isFound = possibleNeighbours.Any(possibleNeighbour => possibleTilesInNeighbour[i].TileId == possibleNeighbour.Id);
                if (isFound)
                    continue;
                if (!deleteSet.Add(i))
                    Debug.LogError("trying to add index for delete second time");
            }

            var deleteList = deleteSet.ToList();
            deleteList.Sort((a, b) => b - a);
            foreach (var i in deleteList)
                possibleTilesInNeighbour.RemoveAt(i);
        }

        private int GetLowestEntropyCellIdx()
        {
            int lowestEntropyTileIdx = -1;
            int lowestEntropy = int.MaxValue;
            for (int i = 0; i < Grid.Length; i++)
            {
                bool suitableEntropy = Grid[i].Entropy < lowestEntropy;
                //suitableEntropy |= Grid[i].Entropy == lowestEntropy && Random.value > 0.5f;
                if (Grid[i].CollapsedTile == null &&
                    Grid[i].AvailableTiles.Count > 0 &&
                    suitableEntropy)
                {
                    lowestEntropyTileIdx = i;
                    lowestEntropy = Grid[i].Entropy;
                }
            }

            return lowestEntropyTileIdx;
        }

        private void Undo()
        {
            if (_history.Count == 0)
                return;
            
            var idx = _history.Pop();
            var tile = Grid[idx].CollapsedTile;
            if (tile != null)
                Destroy(tile.gameObject);
            Grid[idx] = new Cell(_palette.Palette);

            var x = idx % Dim;
            var y = idx / Dim;
            
            var upIdx = x + (y - 1) * Dim;
            if (upIdx > 0 && upIdx < Grid.Length)
                RestoreAvailableTiles(upIdx);

            var rightIdx = (x + 1) + y * Dim;
            if (rightIdx > 0 && rightIdx < Grid.Length)
                RestoreAvailableTiles(rightIdx);
            
            var downIdx = x + (y + 1) * Dim;
            if (downIdx > 0 && downIdx < Grid.Length)
                RestoreAvailableTiles(downIdx);//down
            
            var leftIdx = (x - 1) + y * Dim;
            if (leftIdx > 0 && leftIdx < Grid.Length)
                RestoreAvailableTiles(leftIdx);//left
        }

        private void RestoreAvailableTiles(int idx)
        {
            var x = idx % Dim;
            var y = idx / Dim;
            var upIdx = x + (y - 1) * Dim;
            var rightIdx = (x + 1) + y * Dim;
            var downIdx = x + (y + 1) * Dim;
            var leftIdx = (x - 1) + y * Dim;

            Grid[idx].AvailableTiles = _palette.Palette;


            Tile neighbourTile = null;
            Dictionary<ETileSide, List<PossibleNeighbour>> tileNeighbours = null;
            if (upIdx > 0 && upIdx < Grid.Length)
            {
                neighbourTile = Grid[upIdx].CollapsedTile;
                tileNeighbours = _analyzer.Pattern[neighbourTile.TileId].Neighbours;
                if (neighbourTile != null)
                    RemoveAvailableTiles(idx, tileNeighbours[ETileSide.Down]);
            }

            if (rightIdx > 0 && rightIdx < Grid.Length)
            {
                neighbourTile = Grid[rightIdx].CollapsedTile;
                tileNeighbours = _analyzer.Pattern[neighbourTile.TileId].Neighbours;
                if (neighbourTile != null)
                    RemoveAvailableTiles(idx, tileNeighbours[ETileSide.Left]);
            }
            
            if (downIdx > 0 && downIdx < Grid.Length)
            {
                neighbourTile = Grid[downIdx].CollapsedTile;
                tileNeighbours = _analyzer.Pattern[neighbourTile.TileId].Neighbours;
                if (neighbourTile != null)
                    RemoveAvailableTiles(idx, tileNeighbours[ETileSide.Up]);
            }
            
            if (leftIdx > 0 && leftIdx < Grid.Length)
            {
                neighbourTile = Grid[leftIdx].CollapsedTile;
                tileNeighbours = _analyzer.Pattern[neighbourTile.TileId].Neighbours;
                if (neighbourTile != null)
                    RemoveAvailableTiles(idx, tileNeighbours[ETileSide.Right]);
            }
        }
    }
}