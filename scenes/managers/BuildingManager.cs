using Game.Building;
using Game.Resources.Building;
using Game.Ui;
using Godot;

namespace Game.Manager;

public partial class BuildingManager : Node
{
    private readonly StringName ACTION_LEFT_CLICK = "left_click";
    private readonly StringName ACTION_CANCEL = "cancel";

    [Export]
    private PackedScene buildingGhostScene;

    [Export]
    private GameUi gameUi;

    [Export]
    private GridManager gridManager;

    [Export]
    private Node2D ySortRoot;

    private int startingResourceCount = 4;
    private int currentResourceCount;
    private int currentlyUsedResourceCount;
    private BuildingResource toPlaceBuildingResource;
    private Vector2I? hoveredGridCell;
    private BuildingGhost buildingGhost;

    private int AvailableResourceCount =>
        startingResourceCount + currentResourceCount - currentlyUsedResourceCount;

    public override void _Ready()
    {
        gameUi.BuildingResourceSelected += OnBuildingResourceSelected;
        gridManager.ResourceTilesUpdated += OnResourceTilesUpdated;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed(ACTION_CANCEL))
        {
            ClearBuildingGhost();
        }
        else if (
            @event.IsActionPressed(ACTION_LEFT_CLICK)
            && hoveredGridCell.HasValue
            && toPlaceBuildingResource != null
            && IsBuildingPlacableAtTile(hoveredGridCell.Value)
        )
        {
            PlaceBuildingAtHoveredCellPosition();
        }
    }

    public override void _Process(double delta)
    {
        if (!IsInstanceValid(buildingGhost))
        {
            return;
        }

        var gridPosition = gridManager.GetMouseGridCellPosition();
        buildingGhost.GlobalPosition = gridPosition * 64;

        if (
            toPlaceBuildingResource != null
            && (!hoveredGridCell.HasValue || hoveredGridCell.Value != gridPosition)
        )
        {
            hoveredGridCell = gridPosition;
            UpdateGridDisplay();
        }
    }

    private void UpdateGridDisplay()
    {
        if (hoveredGridCell == null)
        {
            return;
        }

        gridManager.ClearHighlightedTiles();
        gridManager.HighlightBuildableTiles();

        if (IsBuildingPlacableAtTile(hoveredGridCell.Value))
        {
            gridManager.HighlightExpandedBuildableTiles(
                hoveredGridCell.Value,
                toPlaceBuildingResource.BuildableRadius
            );
            gridManager.HighlightResourceTiles(
                hoveredGridCell.Value,
                toPlaceBuildingResource.ResourceRadius
            );
            buildingGhost.SetValid();
        }
        else
        {
            buildingGhost.SetInvalid();
        }
    }

    private void PlaceBuildingAtHoveredCellPosition()
    {
        if (!hoveredGridCell.HasValue)
        {
            return;
        }

        var building = toPlaceBuildingResource.BuildingScene.Instantiate<Node2D>();
        ySortRoot.AddChild(building);

        building.GlobalPosition = hoveredGridCell.Value * 64;

        currentlyUsedResourceCount += toPlaceBuildingResource.ResourceCost;

        ClearBuildingGhost();
    }

    private void ClearBuildingGhost()
    {
        hoveredGridCell = null;

        gridManager.ClearHighlightedTiles();

        if (IsInstanceValid(buildingGhost))
        {
            buildingGhost.QueueFree();
        }

        buildingGhost = null;
    }

    private bool IsBuildingPlacableAtTile(Vector2I tilePosition)
    {
        return gridManager.IsTilePositionBuildable(tilePosition)
            && toPlaceBuildingResource.ResourceCost <= AvailableResourceCount;
    }

    private void OnBuildingResourceSelected(BuildingResource buildingResource)
    {
        if (IsInstanceValid(buildingGhost))
        {
            buildingGhost.QueueFree();
        }

        buildingGhost = buildingGhostScene.Instantiate<BuildingGhost>();
        ySortRoot.AddChild(buildingGhost);

        var buildingSprite = buildingResource.SpriteScene.Instantiate<Sprite2D>();
        buildingGhost.AddChild(buildingSprite);

        toPlaceBuildingResource = buildingResource;
        UpdateGridDisplay();
    }

    private void OnResourceTilesUpdated(int collectedResourceTilesCount)
    {
        currentResourceCount = collectedResourceTilesCount;
    }
}
