using UnityEngine;

namespace WFC
{
    //sides in clockwise order
    public enum ETileSide
    {
        Up,
        UpRight,
        Right,
        DownRight,
        Down,
        DownLeft,
        Left,
        UpLeft
    }
    
    public class Tile : MonoBehaviour
    {
        public int TileId;
    }
}