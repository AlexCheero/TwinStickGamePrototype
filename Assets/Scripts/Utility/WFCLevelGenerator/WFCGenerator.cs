using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    public class WFCGenerator : MonoBehaviour
    {
        public List<Tile> Palette;
        public int Dim;
        public Cell[] Grid;

        void Awake()
        {
            Grid = new Cell[Dim * Dim];
            for (int i = 0; i < Grid.Length; i++)
                Grid[i] = new Cell(Palette);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.D))
            {
                for (var i = 0; i < Grid.Length; i++)
                {
                    var tile = Grid[i].CollapsedTile;
                    if (tile != null)
                        Destroy(tile.gameObject);
                    Grid[i] = new Cell(Palette);
                }
                
                Generate();
            }
        }

        public void Generate()
        {
            var ctr = 0;
            var idx = GetLowestEntropyCellIdx();
            while (idx >= 0)
            {
                var halfDim = Dim / 2;
                var x = idx % Dim;
                var y = idx / Dim;
                var position = new Vector3(x - halfDim, 0, y - halfDim);
                Grid[idx].Collapse(position);
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

                    RemoveAvailableTiles(x + (y - 1) * Dim, tile.AvailableNeighbours(ETileDirection.Up));
                    RemoveAvailableTiles((x + 1) + y * Dim, tile.AvailableNeighbours(ETileDirection.Right));
                    RemoveAvailableTiles(x + (y + 1) * Dim, tile.AvailableNeighbours(ETileDirection.Down));
                    RemoveAvailableTiles((x - 1) + y * Dim, tile.AvailableNeighbours(ETileDirection.Left));
                }
                
                idx = GetLowestEntropyCellIdx();
                ctr++;

                if (ctr >= 10000)
                {
                    Debug.LogError("infinte loop");
                    break;
                }
            }
        }
        
        void RemoveAvailableTiles(int idx, ETileType[] availableNeighbours)
        {
            if (idx < 0 || idx >= Grid.Length)
                return;
            
            var deleteSet = new HashSet<int>();
            var possibleTilesInNeighbour = Grid[idx].AvailableTiles;
                    
            for (int i = 0; i < possibleTilesInNeighbour.Count; i++)
            {
                bool isFound = false;
                foreach (var availableNeighbourType in availableNeighbours)
                {
                    if (possibleTilesInNeighbour[i].Type == availableNeighbourType)
                    {
                        isFound = true;
                        break;
                    }
                }

                if (!isFound)
                {
                    if (!deleteSet.Add(i))
                        Debug.LogError("trying to add index for delete second time");
                }
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
                if (Grid[i].CollapsedTile == null &&
                    Grid[i].AvailableTiles.Count > 0 &&
                    Grid[i].Entropy < lowestEntropy)
                {
                    lowestEntropyTileIdx = i;
                    lowestEntropy = Grid[i].Entropy;
                }
            }

            return lowestEntropyTileIdx;
        }
    }
}