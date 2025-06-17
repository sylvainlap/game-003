using System;
using System.Collections.Generic;
using System.Linq;
using Game.Autoload;
using Game.Component;
using Game.Level.Util;
using Godot;

namespace Game.Manager;

public partial class GridManager : Node
{
    private const string IS_BUILDABLE = "is_buildable";
    private const string IS_IGNORED = "is_ignored";
    private const string IS_WOOD = "is_wood";

    [Signal]
    public delegate void ResourceTilesUpdatedEventHandler(int collectedResourceTilesCount);

    [Signal]
    public delegate void GridStateUpdatedEventHandler();

    private HashSet<Vector2I> validBuildableTiles = new();
    private HashSet<Vector2I> validBuildableAttackTiles = new();
    private HashSet<Vector2I> allTilesInBuildingRadius = new();
    private HashSet<Vector2I> collectedResourceTiles = new();
    private HashSet<Vector2I> occupiedTiles = new();
    private HashSet<Vector2I> goblinOccupiedTiles = new();
    private HashSet<Vector2I> attackTiles = new();

    [Export]
    private TileMapLayer highlightTileMapLayer;

    [Export]
    private TileMapLayer baseTerrainTileMapLayer;

    private List<TileMapLayer> allTileMapLayers = new();
    private Dictionary<TileMapLayer, ElevationLayer> tileMapLayerToElevationLayer = new();

    public override void _Ready()
    {
        GameEvents.Instance.Connect(
            GameEvents.SignalName.BuildingPlaced,
            Callable.From<BuildingComponent>(OnBuildingPlaced)
        );
        GameEvents.Instance.Connect(
            GameEvents.SignalName.BuildingDestroyed,
            Callable.From<BuildingComponent>(OnBuildingDestroyed)
        );
        GameEvents.Instance.Connect(
            GameEvents.SignalName.BuildingEnabled,
            Callable.From<BuildingComponent>(OnBuildingEnabled)
        );
        GameEvents.Instance.Connect(
            GameEvents.SignalName.BuildingDisabled,
            Callable.From<BuildingComponent>(OnBuildingDisabled)
        );

        allTileMapLayers = GetAllTileMapLayers(baseTerrainTileMapLayer);
        MapTileMapLayersToElevationLayers();
    }

    public void HighlightGoblinOccupiedTiles()
    {
        var atlasCoords = new Vector2I(2, 0);

        foreach (var tilePosition in goblinOccupiedTiles)
        {
            highlightTileMapLayer.SetCell(tilePosition, 0, atlasCoords);
        }
    }

    public void HighlightBuildableTiles(bool isAttackTiles = false)
    {
        var atlasCoords = new Vector2I(0, 0);

        foreach (var tilePosition in GetBuildableTileSet(isAttackTiles))
        {
            highlightTileMapLayer.SetCell(tilePosition, 0, atlasCoords);
        }
    }

    public void HighlightExpandedBuildableTiles(Rect2I tileArea, int radius)
    {
        var tiles = GetValidTilesInRadius(tileArea, radius)
            .ToHashSet()
            .Except(validBuildableTiles)
            .Except(occupiedTiles);

        var atlasCoords = new Vector2I(1, 0);

        foreach (var tile in tiles)
        {
            highlightTileMapLayer.SetCell(tile, 0, atlasCoords);
        }
    }

    public void HighlightAttackTiles(Rect2I tileArea, int radius)
    {
        var buildingAreaTiles = tileArea.ToTiles().ToHashSet();

        var tiles = GetValidTilesInRadius(tileArea, radius)
            .ToHashSet()
            .Except(validBuildableAttackTiles)
            .Except(buildingAreaTiles);

        var atlasCoords = new Vector2I(1, 0);

        foreach (var tile in tiles)
        {
            highlightTileMapLayer.SetCell(tile, 0, atlasCoords);
        }
    }

    public void HighlightResourceTiles(Rect2I tileArea, int radius)
    {
        var tiles = GetResourceTilesInRadius(tileArea, radius).ToHashSet();

        var atlasCoords = new Vector2I(1, 0);

        foreach (var tile in tiles)
        {
            highlightTileMapLayer.SetCell(tile, 0, atlasCoords);
        }
    }

    public bool IsTilePositionInAnyBuildingRadius(Vector2I tilePosition)
    {
        return allTilesInBuildingRadius.Contains(tilePosition);
    }

    public bool IsTileAreaBuildable(Rect2I tileArea, bool isAttackTiles = false)
    {
        var tiles = tileArea.ToTiles();

        if (tiles.Count == 0)
        {
            return false;
        }

        (TileMapLayer firstTileMapLayer, _) = GetTileCustomData(tiles[0], IS_BUILDABLE);
        var targetElevationLayer =
            firstTileMapLayer != null ? tileMapLayerToElevationLayer[firstTileMapLayer] : null;

        var tileSetToCheck = GetBuildableTileSet(isAttackTiles);

        if (isAttackTiles)
        {
            tileSetToCheck = tileSetToCheck.Except(occupiedTiles).ToHashSet();
        }

        return tiles.All(
            (tilePosition) =>
            {
                (TileMapLayer tileMapLayer, bool isBuildable) = GetTileCustomData(
                    tilePosition,
                    IS_BUILDABLE
                );

                var elevationLayer =
                    tileMapLayer != null ? tileMapLayerToElevationLayer[tileMapLayer] : null;

                return isBuildable
                    && tileSetToCheck.Contains(tilePosition)
                    && elevationLayer == targetElevationLayer;
            }
        );
    }

    public void ClearHighlightedTiles()
    {
        highlightTileMapLayer.Clear();
    }

    public Vector2I GetMouseGridCellPositionWithDimensionOffset(Vector2 dimensions)
    {
        var mouseGridPosition = highlightTileMapLayer.GetGlobalMousePosition() / 64;
        mouseGridPosition -= dimensions / 2;
        mouseGridPosition = mouseGridPosition.Round();
        return new Vector2I((int)mouseGridPosition.X, (int)mouseGridPosition.Y);
    }

    public Vector2I GetMouseGridCellPosition()
    {
        return ConvertWorldPositionToTilePosition(highlightTileMapLayer.GetGlobalMousePosition());
    }

    public Vector2I ConvertWorldPositionToTilePosition(Vector2 worldPosition)
    {
        var tilePosition = worldPosition / 64;
        tilePosition = tilePosition.Floor();
        return new Vector2I((int)tilePosition.X, (int)tilePosition.Y);
    }

    private HashSet<Vector2I> GetBuildableTileSet(bool isAttackTiles = false)
    {
        return isAttackTiles ? validBuildableAttackTiles : validBuildableTiles;
    }

    private void UpdateGoblinOccupiedTiles(BuildingComponent buildingComponent)
    {
        occupiedTiles.UnionWith(buildingComponent.GetOccupiedCellPositions());

        if (buildingComponent.IsDisabled)
        {
            return;
        }

        var tileArea = buildingComponent.GetTileArea();

        if (buildingComponent.BuildingResource.IsDangerBuilding())
        {
            var tilesInRadius = GetValidTilesInRadius(
                    tileArea,
                    buildingComponent.BuildingResource.DangerRadius
                )
                .ToHashSet();

            goblinOccupiedTiles.UnionWith(tilesInRadius);
        }

        goblinOccupiedTiles.ExceptWith(occupiedTiles);
    }

    private void UpdateValidBuildableTiles(BuildingComponent buildingComponent)
    {
        occupiedTiles.UnionWith(buildingComponent.GetOccupiedCellPositions());

        var tileArea = buildingComponent.GetTileArea();

        var allTiles = GetTilesInRadius(
            tileArea,
            buildingComponent.BuildingResource.BuildableRadius,
            (_) => true
        );

        allTilesInBuildingRadius.UnionWith(allTiles);

        var validTiles = GetValidTilesInRadius(
            tileArea,
            buildingComponent.BuildingResource.BuildableRadius
        );

        validBuildableTiles.UnionWith(validTiles);
        validBuildableTiles.ExceptWith(occupiedTiles);

        validBuildableAttackTiles.UnionWith(validBuildableTiles);

        validBuildableTiles.ExceptWith(goblinOccupiedTiles);

        EmitSignal(SignalName.GridStateUpdated);
    }

    private void UpdateCollectedResourceTiles(BuildingComponent buildingComponent)
    {
        var tileArea = buildingComponent.GetTileArea();

        var resourceTiles = GetResourceTilesInRadius(
            tileArea,
            buildingComponent.BuildingResource.ResourceRadius
        );

        var oldCollectedResourceTilesCount = collectedResourceTiles.Count;
        collectedResourceTiles.UnionWith(resourceTiles);

        if (oldCollectedResourceTilesCount != collectedResourceTiles.Count)
        {
            EmitSignal(SignalName.ResourceTilesUpdated, collectedResourceTiles.Count);
        }

        EmitSignal(SignalName.GridStateUpdated);
    }

    private void UpdateAttackTiles(BuildingComponent buildingComponent)
    {
        if (!buildingComponent.BuildingResource.IsAttackBuilding())
        {
            return;
        }

        var tileArea = buildingComponent.GetTileArea();

        var newAttackTiles = GetTilesInRadius(
                tileArea,
                buildingComponent.BuildingResource.AttackRadius,
                (_) => true
            )
            .ToHashSet();

        attackTiles.UnionWith(newAttackTiles);
    }

    private void RecalculateGrid()
    {
        occupiedTiles.Clear();
        validBuildableTiles.Clear();
        validBuildableAttackTiles.Clear();
        allTilesInBuildingRadius.Clear();
        collectedResourceTiles.Clear();
        goblinOccupiedTiles.Clear();
        attackTiles.Clear();

        var buildingComponents = BuildingComponent.GetValidBuildingComponents(this);

        foreach (var buildingComponent in buildingComponents)
        {
            UpdateBuildingComponentGridState(buildingComponent);
        }

        CheckGoblinCampDestruction();

        EmitSignal(SignalName.ResourceTilesUpdated, collectedResourceTiles.Count);
        EmitSignal(SignalName.GridStateUpdated);
    }

    private void RecalculateGoblinOccupiedTiles()
    {
        goblinOccupiedTiles.Clear();

        var dangerBuildings = BuildingComponent.GetDangerBuildingComponents(this);

        foreach (var building in dangerBuildings)
        {
            UpdateGoblinOccupiedTiles(building);
        }
    }

    private void CheckGoblinCampDestruction()
    {
        var dangerBuildings = BuildingComponent.GetDangerBuildingComponents(this);
        foreach (var building in dangerBuildings)
        {
            var tileArea = building.GetTileArea();
            var isInsideAttackTiles = tileArea
                .ToTiles()
                .Any((tilePosition) => attackTiles.Contains(tilePosition));

            if (isInsideAttackTiles)
            {
                building.Disable();
            }
            else
            {
                building.Enable();
            }
        }
    }

    private List<Vector2I> GetTilesInRadius(
        Rect2I tileArea,
        int radius,
        Func<Vector2I, bool> filterFn
    )
    {
        var result = new List<Vector2I>();

        var tileAreaF = tileArea.ToRect2F();
        var tileAreaCenter = tileAreaF.GetCenter();
        var radiusMod = MathF.Max(tileAreaF.Size.X, tileAreaF.Size.Y) / 2;

        for (var x = tileArea.Position.X - radius; x < tileArea.End.X + radius; x++)
        {
            for (var y = tileArea.Position.Y - radius; y < tileArea.End.Y + radius; y++)
            {
                var tilePosition = new Vector2I(x, y);

                if (
                    !IsTileInsideCircle(tileAreaCenter, tilePosition, radius + radiusMod)
                    || !filterFn(tilePosition)
                )
                    continue;

                result.Add(tilePosition);
            }
        }

        return result;
    }

    private bool IsTileInsideCircle(Vector2 centerPosition, Vector2 tilePosition, float radius)
    {
        float distanceX = centerPosition.X - (tilePosition.X + (float)0.5);
        float distanceY = centerPosition.Y - (tilePosition.Y + (float)0.5);

        float distanceSquared = (distanceX * distanceX) + (distanceY * distanceY);

        return distanceSquared <= (radius * radius);
    }

    private List<Vector2I> GetValidTilesInRadius(Rect2I tileArea, int radius)
    {
        return GetTilesInRadius(
            tileArea,
            radius,
            (tilePosition) =>
            {
                return GetTileCustomData(tilePosition, IS_BUILDABLE).Item2;
            }
        );
    }

    private List<Vector2I> GetResourceTilesInRadius(Rect2I tileArea, int radius)
    {
        return GetTilesInRadius(
            tileArea,
            radius,
            (tilePosition) =>
            {
                return GetTileCustomData(tilePosition, IS_WOOD).Item2;
            }
        );
    }

    private (TileMapLayer, bool) GetTileCustomData(Vector2I tilePosition, string dataName)
    {
        foreach (var layer in allTileMapLayers)
        {
            var customData = layer.GetCellTileData(tilePosition);
            if (customData == null || (bool)customData.GetCustomData(IS_IGNORED))
                continue;

            return (layer, (bool)customData.GetCustomData(dataName));
        }

        return (null, false);
    }

    private List<TileMapLayer> GetAllTileMapLayers(Node2D rootNode)
    {
        var result = new List<TileMapLayer>();

        var children = rootNode.GetChildren();
        children.Reverse();

        foreach (var child in children)
        {
            if (child is Node2D childNode)
            {
                result.AddRange(GetAllTileMapLayers(childNode));
            }
        }

        if (rootNode is TileMapLayer tileMapLayer)
        {
            result.Add(tileMapLayer);
        }

        return result;
    }

    private void MapTileMapLayersToElevationLayers()
    {
        foreach (var layer in allTileMapLayers)
        {
            ElevationLayer elevationLayer;
            Node startNode = layer;
            do
            {
                var parent = startNode.GetParent();
                elevationLayer = parent as ElevationLayer;
                startNode = parent;
            } while (elevationLayer == null && startNode != null);

            tileMapLayerToElevationLayer[layer] = elevationLayer;
        }
    }

    private void UpdateBuildingComponentGridState(BuildingComponent buildingComponent)
    {
        UpdateGoblinOccupiedTiles(buildingComponent);
        UpdateValidBuildableTiles(buildingComponent);
        UpdateCollectedResourceTiles(buildingComponent);
        UpdateAttackTiles(buildingComponent);
    }

    private void OnBuildingPlaced(BuildingComponent buildingComponent)
    {
        UpdateBuildingComponentGridState(buildingComponent);
        CheckGoblinCampDestruction();
    }

    private void OnBuildingDestroyed(BuildingComponent buildingComponent)
    {
        RecalculateGrid();
    }

    private void OnBuildingEnabled(BuildingComponent buildingComponent)
    {
        UpdateBuildingComponentGridState(buildingComponent);
    }

    private void OnBuildingDisabled(BuildingComponent buildingComponent)
    {
        RecalculateGrid();
    }
}
