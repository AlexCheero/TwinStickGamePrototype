
using System;
using UnityEngine;
using WFC;

public static class WFCHelper
{
    public static void ForEachSide(Action<ETileSide, int, int> action, bool isEightDirection = true)
    {
        for (int i = 0; i < 9; i++)
        {
            if (!isEightDirection && i % 2 == 0)
                continue;
            var side = (ETileSide)i;
            if (side == ETileSide.Center)
                continue;
                
            var x = i % 3 - 1;
            var y = i / 3 - 1;

            action(side, x, y);
        }
    }
    
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
}
