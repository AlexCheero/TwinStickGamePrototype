using System;
using System.Collections.Generic;
using UnityEngine;

namespace WFC
{
    public enum ETileDirection
    {
        Up,
        Right,
        Down,
        Left
    }

    public enum ETileType
    {
        Up,
        Right,
        Down,
        Left,
        Blank
    }
    
    public class Tile : MonoBehaviour
    {
        public float Chance;
        //public Dictionary<ETileDirection, ETileType[]> AvailableNeighbours;
        public ETileType Type;

        public ETileType[] AvailableNeighbours(ETileDirection direction)
        {
            return Type switch
            {
                ETileType.Up => direction switch
                {
                    ETileDirection.Up => new[] { ETileType.Right, ETileType.Down, ETileType.Left },
                    ETileDirection.Right => new[] { ETileType.Down, ETileType.Left, ETileType.Up },
                    ETileDirection.Down => new[] { ETileType.Down, ETileType.Blank },
                    ETileDirection.Left => new[] { ETileType.Right, ETileType.Down, ETileType.Up },
                    _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
                },
                ETileType.Down => direction switch
                {
                    ETileDirection.Up => new[] { ETileType.Blank, ETileType.Up },
                    ETileDirection.Right => new[] { ETileType.Up, ETileType.Left, ETileType.Down },
                    ETileDirection.Down => new[] { ETileType.Up, ETileType.Right, ETileType.Left },
                    ETileDirection.Left => new[] { ETileType.Right, ETileType.Up, ETileType.Down },
                    _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
                },
                ETileType.Right => direction switch
                {
                    ETileDirection.Up => new[] { ETileType.Down, ETileType.Left, ETileType.Right },
                    ETileDirection.Right => new[] { ETileType.Down, ETileType.Left, ETileType.Up },
                    ETileDirection.Down => new[] { ETileType.Up, ETileType.Left, ETileType.Right },
                    ETileDirection.Left => new[] { ETileType.Blank, ETileType.Left },
                    _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
                },
                ETileType.Left => direction switch
                {
                    ETileDirection.Up => new[] { ETileType.Right, ETileType.Down, ETileType.Left },
                    ETileDirection.Right => new[] { ETileType.Blank, ETileType.Right },
                    ETileDirection.Down => new[] { ETileType.Up, ETileType.Right, ETileType.Left },
                    ETileDirection.Left => new[] { ETileType.Right, ETileType.Up, ETileType.Down },
                    _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
                },
                ETileType.Blank => direction switch
                {
                    ETileDirection.Up => new[] { ETileType.Up, ETileType.Blank },
                    ETileDirection.Right => new[] { ETileType.Right, ETileType.Blank },
                    ETileDirection.Down => new[] { ETileType.Down, ETileType.Blank },
                    ETileDirection.Left => new[] { ETileType.Left, ETileType.Blank },
                    _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
                },
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}