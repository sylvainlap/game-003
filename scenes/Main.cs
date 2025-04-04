using Game.Manager;
using Game.Resources.Building;
using Godot;

namespace Game;

public partial class Main : Node
{
    private GridManager gridManager;
    private Sprite2D cursor;
    private BuildingResource towerResource;
    private BuildingResource villageResource;
    private Button placeTowerButton;
    private Button placeVillageButton;
    private Vector2I? hoveredGridCell;
    private Node2D ySortRoot;
    private BuildingResource toPlaceBuildingResource;

    public override void _Ready()
    {
        gridManager = GetNode<GridManager>("GridManager");
        cursor = GetNode<Sprite2D>("Cursor");
        towerResource = GD.Load<BuildingResource>("res://resources/buildings/tower.tres");
        villageResource = GD.Load<BuildingResource>("res://resources/buildings/village.tres");
        placeTowerButton = GetNode<Button>("PlaceTowerButton");
        placeVillageButton = GetNode<Button>("PlaceVillageButton");
        ySortRoot = GetNode<Node2D>("YSortRoot");

        cursor.Visible = false;

        placeTowerButton.Pressed += OnPlaceTowerButtonPressed;
        placeVillageButton.Pressed += OnPlaceVillageButtonPressed;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (
            hoveredGridCell.HasValue
            && @event.IsActionPressed("left_click")
            && gridManager.IsTilePositionBuildable(hoveredGridCell.Value)
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
            gridManager.HighlightExpandedBuildableTiles(
                hoveredGridCell.Value,
                toPlaceBuildingResource.BuildableRadius
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
    }

    private void OnPlaceTowerButtonPressed()
    {
        toPlaceBuildingResource = towerResource;
        cursor.Visible = true;
        gridManager.HighlightBuildableTiles();
    }

    private void OnPlaceVillageButtonPressed()
    {
        toPlaceBuildingResource = villageResource;
        cursor.Visible = true;
        gridManager.HighlightBuildableTiles();
    }
}
