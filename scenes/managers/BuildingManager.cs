using Game.Resources.Building;
using Game.Ui;
using Godot;

namespace Game.Manager;

public partial class BuildingManager : Node
{
    [Export]
    private Sprite2D cursor;

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

    private int AvailableResourceCount =>
        startingResourceCount + currentResourceCount - currentlyUsedResourceCount;

    public override void _Ready()
    {
        cursor.Visible = false;

        gameUi.BuildingResourceSelected += OnBuildingResourceSelected;
        gridManager.ResourceTilesUpdated += OnResourceTilesUpdated;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (
            hoveredGridCell.HasValue
            && toPlaceBuildingResource != null
            && @event.IsActionPressed("left_click")
            && gridManager.IsTilePositionBuildable(hoveredGridCell.Value)
            && toPlaceBuildingResource.ResourceCost <= AvailableResourceCount
        )
        {
            PlaceBuildingAtHoveredCellPosition();
            cursor.Visible = false;
        }
    }

    public override void _Process(double delta)
    {
        var gridPosition = gridManager.GetMouseGridCellPosition();
        cursor.GlobalPosition = gridPosition * 64;
        if (
            toPlaceBuildingResource != null
            && cursor.Visible
            && (!hoveredGridCell.HasValue || hoveredGridCell.Value != gridPosition)
        )
        {
            hoveredGridCell = gridPosition;

            gridManager.ClearHighlightedTiles();
            gridManager.HighlightExpandedBuildableTiles(
                hoveredGridCell.Value,
                toPlaceBuildingResource.BuildableRadius
            );
            gridManager.HighlightResourceTiles(
                hoveredGridCell.Value,
                toPlaceBuildingResource.ResourceRadius
            );
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

        hoveredGridCell = null;
        gridManager.ClearHighlightedTiles();

        currentlyUsedResourceCount += toPlaceBuildingResource.ResourceCost;
    }

    private void OnBuildingResourceSelected(BuildingResource buildingResource)
    {
        toPlaceBuildingResource = buildingResource;
        cursor.Visible = true;
        gridManager.HighlightBuildableTiles();
    }

    private void OnResourceTilesUpdated(int collectedResourceTilesCount)
    {
        currentResourceCount = collectedResourceTilesCount;
    }
}
