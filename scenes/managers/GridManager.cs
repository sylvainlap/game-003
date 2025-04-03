using System.Collections.Generic;
using System.Linq;
using Game.Autoload;
using Game.Component;
using Godot;

namespace Game.Manager;

public partial class GridManager : Node
{
    private HashSet<Vector2I> validBuildableTiles = new();

    [Export]
    private TileMapLayer highlightTileMapLayer;

    [Export]
    private TileMapLayer baseTerrainTileMapLayer;

    public override void _Ready()
    {
        GameEvents.Instance.BuildingPlaced += OnBuildingPlaced;
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
        ClearHighlightedTiles();
        HighlightBuildableTiles();

        var validTiles = GetValidTilesInRadius(rootCell, radius).ToHashSet();
        var expandedTiles = validTiles.Except(validBuildableTiles).Except(GetOccupiedTiles());
        var atlasCoords = new Vector2I(1, 0);

        foreach (var expandedTile in expandedTiles)
        {
            highlightTileMapLayer.SetCell(expandedTile, 0, atlasCoords);
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
        var validTiles = GetValidTilesInRadius(
            buildingComponent.GetGridCellPosition(),
            buildingComponent.BuildableRadius
        );

        validBuildableTiles.UnionWith(validTiles);
        validBuildableTiles.ExceptWith(GetOccupiedTiles());
    }

    private List<Vector2I> GetValidTilesInRadius(Vector2I rootCell, int radius)
    {
        var result = new List<Vector2I>();

        for (var x = rootCell.X - radius; x <= rootCell.X + radius; x++)
        {
            for (var y = rootCell.Y - radius; y <= rootCell.Y + radius; y++)
            {
                var tilePosition = new Vector2I(x, y);

                if (!IsTilePositionValid(tilePosition))
                    continue;

                result.Add(tilePosition);
            }
        }

        return result;
    }

    private bool IsTilePositionValid(Vector2I tilePosition)
    {
        var customData = baseTerrainTileMapLayer.GetCellTileData(tilePosition);

        if (customData == null)
            return false;

        return (bool)customData.GetCustomData("buildable");
    }

    private IEnumerable<Vector2I> GetOccupiedTiles()
    {
        var buildingComponents = GetTree()
            .GetNodesInGroup(nameof(BuildingComponent))
            .Cast<BuildingComponent>();

        return buildingComponents.Select(_ => _.GetGridCellPosition());
    }

    private void OnBuildingPlaced(BuildingComponent buildingComponent)
    {
        UpdateValidBuildableTiles(buildingComponent);
    }
}
