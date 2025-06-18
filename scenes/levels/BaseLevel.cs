using Game.Autoload;
using Game.Manager;
using Game.Resources.Level;
using Game.Ui;
using Godot;

namespace Game;

public partial class BaseLevel : Node
{
    [Export]
    private PackedScene levelCompleteScreenScene;

    [Export]
    private LevelDefinitionResource levelDefinitionResource;

    private Node2D baseBuilding;
    private TileMapLayer baseTerrainTileMapLayer;
    private GameCamera gameCamera;
    private GoldMine goldMine;
    private GridManager gridManager;
    private GameUi gameUi;
    private BuildingManager buildingManager;
    private bool IsCompleted;

    public override void _Ready()
    {
        baseBuilding = GetNode<Node2D>("%Base");
        baseTerrainTileMapLayer = GetNode<TileMapLayer>("%BaseTerrainTileMapLayer");
        gameCamera = GetNode<GameCamera>("GameCamera");
        goldMine = GetNode<GoldMine>("%GoldMine");
        gridManager = GetNode<GridManager>("GridManager");
        gameUi = GetNode<GameUi>("GameUi");
        buildingManager = GetNode<BuildingManager>("BuildingManager");

        gameCamera.SetBoundingRect(baseTerrainTileMapLayer.GetUsedRect());
        gameCamera.CenterOnPosition(baseBuilding.GlobalPosition);

        buildingManager.SetStartingResourceCount(levelDefinitionResource.StartingResourceCount);

        gridManager.GridStateUpdated += OnGridStateUpdated;
    }

    private void ShowLevelComplete()
    {
        IsCompleted = true;
        SaveManager.SaveLevelCompletion(levelDefinitionResource);

        var levelCompleteScene = levelCompleteScreenScene.Instantiate<LevelCompleteScreen>();
        AddChild(levelCompleteScene);
        goldMine.SetActive();
        gameUi.HideUI();
    }

    private void OnGridStateUpdated()
    {
        if (IsCompleted)
        {
            return;
        }

        var goldMineTilePosition = gridManager.ConvertWorldPositionToTilePosition(
            goldMine.GlobalPosition
        );

        if (gridManager.IsTilePositionInAnyBuildingRadius(goldMineTilePosition))
        {
            ShowLevelComplete();
        }
    }
}
