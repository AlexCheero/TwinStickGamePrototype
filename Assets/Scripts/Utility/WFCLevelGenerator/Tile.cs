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

    public enum ETileRotation
    {
        No,
        Two,
        Four
    }
    
    public class Tile : MonoBehaviour
    {
        public int TileId;
        public ETileRotation RotationType;
    }
}