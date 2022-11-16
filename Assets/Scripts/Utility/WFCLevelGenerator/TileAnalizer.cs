using System;
using System.Collections.Generic;
using UnityEngine;
using WFC;

public struct PossibleNeighbour
{
    public int Id;
    public int Count;
    public float Chance;

    public PossibleNeighbour(int id)
    {
        Id = id;
        Count = 1;
        Chance = 0;
    }
}

public class PossibleNeighbours
{
    public Dictionary<ETileSide, List<PossibleNeighbour>> Neighbours;

    public PossibleNeighbours()
    {
        Neighbours = new Dictionary<ETileSide, List<PossibleNeighbour>>()
        {
            { ETileSide.DownLeft, new List<PossibleNeighbour>() },
            { ETileSide.Down, new List<PossibleNeighbour>() },
            { ETileSide.DownRight, new List<PossibleNeighbour>() },
            { ETileSide.Left, new List<PossibleNeighbour>() },
            { ETileSide.Right, new List<PossibleNeighbour>() },
            { ETileSide.UpLeft, new List<PossibleNeighbour>() },
            { ETileSide.Up, new List<PossibleNeighbour>() },
            { ETileSide.UpRight, new List<PossibleNeighbour>() }
        };
    }

    public void Add(ETileSide side, Tile tile)
    {
        if (!Neighbours.ContainsKey(side))
            Neighbours[side] = new List<PossibleNeighbour>();
        var neighboursOnSide = Neighbours[side];
        for (var i = 0; i < neighboursOnSide.Count; i++)
        {
            var neighbour = neighboursOnSide[i];
            if (neighbour.Id != tile.TileId)
                continue;
            neighbour.Count++;
            neighboursOnSide[i] = neighbour;
            return;
        }
        
        neighboursOnSide.Add(new PossibleNeighbour(tile.TileId));

        for (var i = 0; i < neighboursOnSide.Count; i++)
        {
            var neighbour = neighboursOnSide[i];
            neighbour.Chance = neighbour.Count / neighboursOnSide.Count;
            neighboursOnSide[i] = neighbour;
        }
        
#if DEBUG
        var chanceOverall = 0.0f;
        foreach (var neighbour in neighboursOnSide)
            chanceOverall += neighbour.Chance;
        if (chanceOverall > 1)
            throw new Exception("overall chance of tile neighbours is bigger than 1");
#endif
    }
}

[RequireComponent(typeof(TilePlacer))]
[RequireComponent(typeof(TilePalette))]
public class TileAnalizer : MonoBehaviour
{
    [SerializeField]
    private bool _isEightDirectionAnalize;
    
    private TilePlacer _placer;
    private TilePalette _palette;
    
    private List<PossibleNeighbours> _pattern;
    
    void Start()
    {
        _placer = GetComponent<TilePlacer>();
        _palette = GetComponent<TilePalette>();

        //_pattern init should be in Start when all duplicates are already removed from palette
        _pattern = new List<PossibleNeighbours>(_palette.Palette.Count);
        for (int i = 0; i < _palette.Palette.Count; i++)
            _pattern.Add(new PossibleNeighbours());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
            Analize();
    }

    private void Analize()
    {
        foreach (var tile in _placer.PlacedTiles.Values)
        {
            var pos = tile.transform.position;
            for (int i = 0; i < 9; i++)
            {
                if (!_isEightDirectionAnalize && i % 2 == 0)
                    continue;
                var side = (ETileSide)i;
                if (side == ETileSide.Center)
                    continue;
                
                var xDelta = i % 3 - 1;
                var yDelta = i / 3 - 1;
                
                var neighbourPos = pos;
                neighbourPos.x += xDelta * _placer.SnapSize;
                neighbourPos.z += yDelta * _placer.SnapSize;
                if (!_placer.PlacedTiles.ContainsKey(neighbourPos))
                    continue;
                var neighbour = _placer.PlacedTiles[neighbourPos];
                
                _pattern[tile.TileId].Add(side, neighbour);
            }
        }
    }
}
