
using System;
using WFC;

public static class WFCHelper
{
    public static void ForEachSide(bool isEightDirection, Action<ETileSide, int, int> action)
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
}
