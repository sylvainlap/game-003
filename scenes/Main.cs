using Game.Manager;
using Godot;

namespace Game;

public partial class Main : Node
{
    private GoldMine goldMine;
    private GridManager gridManager;

    public override void _Ready()
    {
        goldMine = GetNode<GoldMine>("%GoldMine");
        gridManager = GetNode<GridManager>("GridManager");

        gridManager.GridStateUpdated += OnGridStateUpdated;
    }

    private void OnGridStateUpdated()
    {
        var goldMineTilePosition = gridManager.ConvertWorldPositionToTilePosition(
            goldMine.GlobalPosition
        );

        if (gridManager.IsTilePositionBuildable(goldMineTilePosition))
        {
            goldMine.SetActive();
        }
    }
}
