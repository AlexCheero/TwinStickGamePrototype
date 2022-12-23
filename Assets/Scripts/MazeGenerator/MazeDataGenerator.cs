/*
 * written by Joseph Hocking 2017
 * released under MIT license
 * text of license https://opensource.org/licenses/MIT
 */

using Random = UnityEngine.Random;

public static class MazeDataGenerator
{
    public static int[,] FromDimensions(LevelSettingsDigits levelDigits)
    {
        int[,] maze = new int[levelDigits.Rows, levelDigits.Cols];

        int rMax = maze.GetUpperBound(0);
        int cMax = maze.GetUpperBound(1);

        for (int i = 0; i <= rMax; i++)
        {
            for (int j = 0; j <= cMax; j++)
            {
                // outside wall
                if (i == 0 || j == 0 || i == rMax || j == cMax)
                {
                    maze[i, j] = 1;
                } 
                // every other inside space
                else if (i % levelDigits.GapX == 0 && j % levelDigits.GapY == 0 && Random.value > levelDigits.PlacementThreshold)
                {
                    maze[i, j] = 1;

                    // in addition to this spot, randomly place adjacent
                    int a = Random.value < .5 ? 0 : (Random.value < .5 ? -1 : 1);
                    int b = a != 0 ? 0 : (Random.value < .5 ? -1 : 1);
                    maze[i+a, j+b] = 1;
                }
            }
        }

        return maze;
    }
}