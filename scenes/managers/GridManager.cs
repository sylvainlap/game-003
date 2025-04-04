using System;
using System.Collections.Generic;
using System.Linq;
using Game.Autoload;
using Game.Component;
using Godot;

namespace Game.Manager;

public partial class GridManager : Node
{
    private const string IS_BUILDABLE = "is_buildable";
    private const string IS_WOOD = "is_wood";

    [Signal]
    public delegate void ResourceTilesUpdatedEventHandler(int collectedResourceTilesCount);

    private HashSet<Vector2I> validBuildableTiles = new();
    private HashSet<Vector2I> collectedResourceTiles = new();

    [Export]
    private TileMapLayer highlightTileMapLayer;

    [Export]
    private TileMapLayer baseTerrainTileMapLayer;

    private List<TileMapLayer> allTileMapLayers = new();

    public override void _Ready()
    {
        GameEvents.Instance.BuildingPlaced += OnBuildingPlaced;
        allTileMapLayers = GetAllTileMapLayers(baseTerrainTileMapLayer);
    }

    public void HighlightBuildableTiles()
    {
        var atlasCoords = new Vector2I(0, 0);

        foreach (var tilePosition in validBuildableTiles)
        {
            highlightTileMapLayer.SetCell(tilePosition, 0, atlasCoords);
        }
    }

    public void HighlightExpandedBuildableTiles(Vector2I rootCell, int radius)
    {
        HighlightBuildableTiles();

        var tiles = GetValidTilesInRadius(rootCell, radius).ToHashSet();
        var expandedTiles = tiles.Except(validBuildableTiles).Except(GetOccupiedTiles());
        var atlasCoords = new Vector2I(1, 0);

        foreach (var tile in expandedTiles)
        {
            highlightTileMapLayer.SetCell(tile, 0, atlasCoords);
        }
    }

    public void HighlightResourceTiles(Vector2I rootCell, int radius)
    {
        var tiles = GetResourceTilesInRadius(rootCell, radius);
        var atlasCoords = new Vector2I(1, 0);

        foreach (var tile in tiles)
        {
            highlightTileMapLayer.SetCell(tile, 0, atlasCoords);
        }
    }

    public bool IsTilePositionBuildable(Vector2I tilePosition)
    {
        return validBuildableTiles.Contains(tilePosition);
    }

    public void ClearHighlightedTiles()
    {
        highlightTileMapLayer.Clear();
    }

    public Vector2I GetMouseGridCellPosition()
    {
        var mousePosition = highlightTileMapLayer.GetGlobalMousePosition();
        var gridPosition = mousePosition / 64;
        gridPosition = gridPosition.Floor();
        return new Vector2I((int)gridPosition.X, (int)gridPosition.Y);
    }

    private void UpdateValidBuildableTiles(BuildingComponent buildingComponent)
    {
        var tiles = GetValidTilesInRadius(
            buildingComponent.GetGridCellPosition(),
            buildingComponent.BuildingResource.BuildableRadius
        );
        validBuildableTiles.UnionWith(tiles);
        validBuildableTiles.ExceptWith(GetOccupiedTiles());
    }

    private void UpdateCollectedResourceTiles(BuildingComponent buildingComponent)
    {
        var tiles = GetResourceTilesInRadius(
            buildingComponent.GetGridCellPosition(),
            buildingComponent.BuildingResource.ResourceRadius
        );

        var oldCollectedResourceTilesCount = collectedResourceTiles.Count;
        collectedResourceTiles.UnionWith(tiles);

        if (oldCollectedResourceTilesCount != collectedResourceTiles.Count)
        {
            EmitSignal(SignalName.ResourceTilesUpdated, collectedResourceTiles.Count);
        }
    }

    private List<Vector2I> GetTilesInRadius(
        Vector2I rootCell,
        int radius,
        Func<Vector2I, bool> filterFn
    )
    {
        var result = new List<Vector2I>();

        for (var x = rootCell.X - radius; x <= rootCell.X + radius; x++)
        {
            for (var y = rootCell.Y - radius; y <= rootCell.Y + radius; y++)
            {
                var tilePosition = new Vector2I(x, y);

                if (!filterFn(tilePosition))
                    continue;

                result.Add(tilePosition);
            }
        }

        return result;
    }

    private List<Vector2I> GetValidTilesInRadius(Vector2I rootCell, int radius)
    {
        return GetTilesInRadius(
            rootCell,
            radius,
            (tilePosition) =>
            {
                return TileHasCustomData(tilePosition, IS_BUILDABLE);
            }
        );
    }

    private List<Vector2I> GetResourceTilesInRadius(Vector2I rootCell, int radius)
    {
        return GetTilesInRadius(
            rootCell,
            radius,
            (tilePosition) =>
            {
                return TileHasCustomData(tilePosition, IS_WOOD);
            }
        );
    }

    private bool TileHasCustomData(Vector2I tilePosition, string dataName)
    {
        foreach (var layer in allTileMapLayers)
        {
            var customData = layer.GetCellTileData(tilePosition);
            if (customData == null)
                continue;

            return (bool)customData.GetCustomData(dataName);
        }

        return false;
    }

    private IEnumerable<Vector2I> GetOccupiedTiles()
    {
        var buildingComponents = GetTree()
            .GetNodesInGroup(nameof(BuildingComponent))
            .Cast<BuildingComponent>();

        return buildingComponents.Select(_ => _.GetGridCellPosition());
    }

    private List<TileMapLayer> GetAllTileMapLayers(TileMapLayer rootTileMapLayer)
    {
        var result = new List<TileMapLayer>();

        var children = rootTileMapLayer.GetChildren();
        children.Reverse();

        foreach (var child in children)
        {
            if (child is TileMapLayer childLayer)
            {
                result.AddRange(GetAllTileMapLayers(childLayer));
            }
        }

        result.Add(rootTileMapLayer);

        return result;
    }

    private void OnBuildingPlaced(BuildingComponent buildingComponent)
    {
        UpdateValidBuildableTiles(buildingComponent);
        UpdateCollectedResourceTiles(buildingComponent);
    }
}
