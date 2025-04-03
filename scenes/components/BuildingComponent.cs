using Game.Autoload;
using Godot;

namespace Game.Component;

public partial class BuildingComponent : Node2D
{
    [Export]
    public int BuildableRadius { get; private set; }

    public override void _Ready()
    {
        AddToGroup(nameof(BuildingComponent));
        GameEvents.EmitBuildingPlaced(this);
    }

    public Vector2I getGridCellPosition()
    {
        var gridPosition = GlobalPosition / 64;
        gridPosition = gridPosition.Floor();
        return new Vector2I((int)gridPosition.X, (int)gridPosition.Y);
    }
}
