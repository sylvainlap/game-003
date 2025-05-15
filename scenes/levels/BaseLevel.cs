using Game.Manager;
using Game.Ui;
using Godot;

namespace Game;

public partial class BaseLevel : Node
{
    [Export]
    private PackedScene levelCompleteScreenScene;

    private Node2D baseBuilding;
    private TileMapLayer baseTerrainTileMapLayer;
    private GameCamera gameCamera;
    private GoldMine goldMine;
    private GridManager gridManager;
    private GameUi gameUi;

    public override void _Ready()
    {
        baseBuilding = GetNode<Node2D>("%Base");
        baseTerrainTileMapLayer = GetNode<TileMapLayer>("%BaseTerrainTileMapLayer");
        gameCamera = GetNode<GameCamera>("GameCamera");
        goldMine = GetNode<GoldMine>("%GoldMine");
        gridManager = GetNode<GridManager>("GridManager");
        gameUi = GetNode<GameUi>("GameUi");

        gameCamera.SetBoundingRect(baseTerrainTileMapLayer.GetUsedRect());
        gameCamera.CenterOnPosition(baseBuilding.GlobalPosition);

        gridManager.GridStateUpdated += OnGridStateUpdated;
    }

    private void OnGridStateUpdated()
    {
        var goldMineTilePosition = gridManager.ConvertWorldPositionToTilePosition(
            goldMine.GlobalPosition
        );

        if (gridManager.IsTilePositionBuildable(goldMineTilePosition))
        {
            var levelCompleteScene = levelCompleteScreenScene.Instantiate<LevelCompleteScreen>();
            AddChild(levelCompleteScene);

            goldMine.SetActive();

            gameUi.HideUI();
        }
    }
}
