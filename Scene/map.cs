using Godot;
using System.Collections.Generic;

public partial class map : Node2D
{
    [Export] private TileMap baseTileMap;
    [Export] private Node2D player;
    
    private Vector2I tileMapSize;
    private Vector2I currentPlayerTileMap = Vector2I.Zero;
    private Dictionary<Vector2I, TileMap> activeTileMaps = new Dictionary<Vector2I, TileMap>();

    public override void _Ready()
    {
        tileMapSize = baseTileMap.GetUsedRect().Size;
        UpdateTileMaps();
    }

    public override void _Process(double delta)
    {
        Vector2I newPlayerTileMap = GetPlayerTileMap();
        if (newPlayerTileMap != currentPlayerTileMap)
        {
            currentPlayerTileMap = newPlayerTileMap;
            UpdateTileMaps();
        }
    }

    private Vector2I GetPlayerTileMap()
    {
        Vector2 playerPos = player.GlobalPosition;
        return new Vector2I(
            Mathf.FloorToInt(playerPos.X / (tileMapSize.X * baseTileMap.CellQuadrantSize)),
            Mathf.FloorToInt(playerPos.Y / (tileMapSize.Y * baseTileMap.CellQuadrantSize))
        );
    }

    private void UpdateTileMaps()
    {
        List<Vector2I> requiredPositions = new List<Vector2I>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                requiredPositions.Add(currentPlayerTileMap + new Vector2I(x, y));
            }
        }

        // Remove unnecessary tilemaps
        List<Vector2I> toRemove = new List<Vector2I>();
        foreach (var kvp in activeTileMaps)
        {
            if (!requiredPositions.Contains(kvp.Key))
            {
                kvp.Value.QueueFree();
                toRemove.Add(kvp.Key);
            }
        }
        foreach (var pos in toRemove)
        {
            activeTileMaps.Remove(pos);
        }

        // Add new tilemaps
        foreach (var pos in requiredPositions)
        {
            if (!activeTileMaps.ContainsKey(pos))
            {
                TileMap newTileMap = (TileMap)baseTileMap.Duplicate();
                newTileMap.Position = new Vector2(
                    pos.X * tileMapSize.X * baseTileMap.CellQuadrantSize,
                    pos.Y * tileMapSize.Y * baseTileMap.CellQuadrantSize
                );
                AddChild(newTileMap);
                activeTileMaps[pos] = newTileMap;
            }
        }
    }
}