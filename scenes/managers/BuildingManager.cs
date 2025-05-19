using System.Linq;
using Game.Building;
using Game.Component;
using Game.Resources.Building;
using Game.Ui;
using Godot;

namespace Game.Manager;

public partial class BuildingManager : Node
{
    private readonly StringName ACTION_CANCEL = "cancel";
    private readonly StringName ACTION_LEFT_CLICK = "left_click";
    private readonly StringName ACTION_RIGHT_CLICK = "right_click";

    [Signal]
    public delegate void AvailableResourceCountChangedEventHandler(int availableResourceCount);

    [Export]
    private PackedScene buildingGhostScene;

    [Export]
    private GameUi gameUi;

    [Export]
    private GridManager gridManager;

    [Export]
    private Node2D ySortRoot;

    private enum State
    {
        Normal,
        PlacingBuilding,
    }

    private int currentResourceCount;
    private int currentlyUsedResourceCount;
    private BuildingResource toPlaceBuildingResource;
    private Rect2I hoveredGridArea = new(Vector2I.Zero, Vector2I.One);
    private BuildingGhost buildingGhost;
    private State currentState;
    private int startingResourceCount;

    private int AvailableResourceCount =>
        startingResourceCount + currentResourceCount - currentlyUsedResourceCount;

    public override void _Ready()
    {
        gameUi.BuildingResourceSelected += OnBuildingResourceSelected;
        gridManager.ResourceTilesUpdated += OnResourceTilesUpdated;

        Callable
            .From(
                () => EmitSignal(SignalName.AvailableResourceCountChanged, AvailableResourceCount)
            )
            .CallDeferred();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        switch (currentState)
        {
            case State.Normal:
                if (@event.IsActionPressed(ACTION_RIGHT_CLICK))
                {
                    DestroyBuildingAtHoveredCellPosition();
                }
                break;
            case State.PlacingBuilding:
                if (@event.IsActionPressed(ACTION_CANCEL))
                {
                    ChangeState(State.Normal);
                }
                else if (
                    @event.IsActionPressed(ACTION_LEFT_CLICK)
                    && toPlaceBuildingResource != null
                    && IsBuildingPlacableAtArea(hoveredGridArea)
                )
                {
                    PlaceBuildingAtHoveredCellPosition();
                }
                break;
        }
    }

    public override void _Process(double delta)
    {
        var mouseGridPosition = gridManager.GetMouseGridCellPosition();

        switch (currentState)
        {
            case State.Normal:
                break;
            case State.PlacingBuilding:
                buildingGhost.GlobalPosition = mouseGridPosition * 64;
                break;
        }

        var rootCell = hoveredGridArea.Position;

        if (rootCell != mouseGridPosition)
        {
            hoveredGridArea.Position = mouseGridPosition;
            UpdateHoveredGridArea();
        }
    }

    public void SetStartingResourceCount(int count)
    {
        startingResourceCount = count;
    }

    private void UpdateGridDisplay()
    {
        gridManager.ClearHighlightedTiles();
        gridManager.HighlightBuildableTiles();

        if (IsBuildingPlacableAtArea(hoveredGridArea))
        {
            gridManager.HighlightExpandedBuildableTiles(
                hoveredGridArea,
                toPlaceBuildingResource.BuildableRadius
            );
            gridManager.HighlightResourceTiles(
                hoveredGridArea,
                toPlaceBuildingResource.ResourceRadius
            );
            buildingGhost.SetValid();
        }
        else
        {
            buildingGhost.SetInvalid();
        }

        buildingGhost.DoHoverAnimation();
    }

    private void PlaceBuildingAtHoveredCellPosition()
    {
        var building = toPlaceBuildingResource.BuildingScene.Instantiate<Node2D>();
        ySortRoot.AddChild(building);

        building.GlobalPosition = hoveredGridArea.Position * 64;

        currentlyUsedResourceCount += toPlaceBuildingResource.ResourceCost;
        EmitSignal(SignalName.AvailableResourceCountChanged, AvailableResourceCount);

        ChangeState(State.Normal);
    }

    private void DestroyBuildingAtHoveredCellPosition()
    {
        var rootCell = hoveredGridArea.Position;
        var buildingComponent = GetTree()
            .GetNodesInGroup(nameof(BuildingComponent))
            .Cast<BuildingComponent>()
            .Where(_ => _.IsTileInBuildingArea(rootCell) && _.BuildingResource.IsDeletable)
            .FirstOrDefault();

        if (buildingComponent == null)
        {
            return;
        }

        currentlyUsedResourceCount -= buildingComponent.BuildingResource.ResourceCost;
        EmitSignal(SignalName.AvailableResourceCountChanged, AvailableResourceCount);

        buildingComponent.Destroy();
    }

    private void ClearBuildingGhost()
    {
        gridManager.ClearHighlightedTiles();

        if (IsInstanceValid(buildingGhost))
        {
            buildingGhost.QueueFree();
        }

        buildingGhost = null;
    }

    private bool IsBuildingPlacableAtArea(Rect2I tileArea)
    {
        var allTilesBuildable = gridManager.IsTileAreaBuildable(tileArea);

        return allTilesBuildable && toPlaceBuildingResource.ResourceCost <= AvailableResourceCount;
    }

    private void UpdateHoveredGridArea()
    {
        switch (currentState)
        {
            case State.Normal:
                break;
            case State.PlacingBuilding:
                UpdateGridDisplay();
                break;
            default:
                break;
        }
    }

    private void ChangeState(State toState)
    {
        switch (currentState)
        {
            case State.Normal:
                break;
            case State.PlacingBuilding:
                ClearBuildingGhost();
                toPlaceBuildingResource = null;
                break;
        }

        currentState = toState;

        switch (currentState)
        {
            case State.Normal:
                break;
            case State.PlacingBuilding:
                buildingGhost = buildingGhostScene.Instantiate<BuildingGhost>();
                ySortRoot.AddChild(buildingGhost);
                break;
        }
    }

    private void OnBuildingResourceSelected(BuildingResource buildingResource)
    {
        ChangeState(State.PlacingBuilding);

        hoveredGridArea.Size = buildingResource.Dimensions;

        var buildingSprite = buildingResource.SpriteScene.Instantiate<Sprite2D>();
        buildingGhost.AddSpriteNode(buildingSprite);
        buildingGhost.SetDimensions(buildingResource.Dimensions);

        toPlaceBuildingResource = buildingResource;

        UpdateGridDisplay();
    }

    private void OnResourceTilesUpdated(int collectedResourceTilesCount)
    {
        currentResourceCount = collectedResourceTilesCount;
        EmitSignal(SignalName.AvailableResourceCountChanged, AvailableResourceCount);
    }
}
