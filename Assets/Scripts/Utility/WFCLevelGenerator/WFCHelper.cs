
using System;
using UnityEngine;
using WFC;

public static class WFCHelper
{
    public static Vector2Int GetNeighbourBias(int i) => new Vector2Int(i % 3 - 1, i / 3 - 1);
    
    public static Vector2Int IdxToGridPos(int idx, int dim) =>
        new Vector2Int
        {
            x = idx % dim,
            y = idx / dim
        };

    public static int GridPosToIdx(Vector2Int pos, int dim)
    {
        if (pos.x < 0 || pos.y < 0 || pos.x >= dim || pos.y >= dim)
            return -1;
        return pos.x + pos.y * dim;
    }

    public static Vector3 GridPosToPos(Vector2Int gridPos, int dim)
    {
        var halfDim = dim / 2;
        return new Vector3(gridPos.x - halfDim, 0, gridPos.y - halfDim);
    }

    public static Vector2Int PosToGridPos(Vector3 pos, int dim)
    {
        var halfDim = dim / 2;
        return new Vector2Int((int)pos.x + halfDim, (int)pos.z + halfDim);
    }

    public static bool IsGridPosValid(Vector2Int gridPos, int dim)
    {
        return gridPos.x >= 0 && gridPos.x < dim && gridPos.y >= 0 && gridPos.y < dim;
    }

    public static ETileSide TurnSide(ETileSide side, int numTurns)
    {
        var intSide = (int)side + numTurns;
        return (ETileSide)(intSide % 8);
    }

    public static ETileSide GetOppositeSide(ETileSide side) => TurnSide(side, 4);
    
    public static float GetTileRotation(this Tile tile) => tile.transform.eulerAngles.y % 360;
}
