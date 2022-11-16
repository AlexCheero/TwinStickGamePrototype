using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WFC;

public class TilePalette : MonoBehaviour
{
    public List<Tile> Palette;

    private void Awake()
    {
        Palette = Palette.Distinct().ToList();
        for (var i = 0; i < Palette.Count; i++)
        {
            var tile = Palette[i];
            tile.TileId = i;
        }
    }
}
