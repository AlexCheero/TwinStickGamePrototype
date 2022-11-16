using UnityEngine;

namespace WFC
{
    public enum ETileSide
    {
        DownLeft,
        Down,
        DownRight,
        Left,
        Center,
        Right,
        UpLeft,
        Up,
        UpRight
    }
    
    public class Tile : MonoBehaviour
    {
        public float Chance;
        public int TileId;
    }
}