using Godot;

namespace Game;

public partial class GoldMine : Node2D
{
    [Export]
    private Texture2D activeTexture;

    private Sprite2D sprite2D;

    public override void _Ready()
    {
        sprite2D = GetNode<Sprite2D>("Sprite2D");
    }

    public void SetActive()
    {
        sprite2D.Texture = activeTexture;
    }
}
