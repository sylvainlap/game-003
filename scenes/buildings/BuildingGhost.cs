using Godot;

namespace Game.Building;

public partial class BuildingGhost : Node2D
{
    private Node2D topLeft;
    private Node2D topRight;
    private Node2D bottomLeft;
    private Node2D bottomRight;
    private Node2D spriteRoot;
    private Node2D upDownRoot;

    private Tween spriteTween;

    public override void _Ready()
    {
        topLeft = GetNode<Node2D>("TopLeft");
        topRight = GetNode<Node2D>("TopRight");
        bottomLeft = GetNode<Node2D>("BottomLeft");
        bottomRight = GetNode<Node2D>("BottomRight");
        spriteRoot = GetNode<Node2D>("SpriteRoot");
        upDownRoot = GetNode<Node2D>("%UpDownRoot");

        var upDownTween = CreateTween();
        upDownTween.SetLoops(0);
        upDownTween
            .TweenProperty(upDownRoot, "position", Vector2.Down * 6, .3)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.InOut);
        upDownTween
            .TweenProperty(upDownRoot, "position", Vector2.Up * 6, .3)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.InOut);
    }

    public void SetInvalid()
    {
        Modulate = Colors.Red;
        spriteRoot.Modulate = Modulate;
    }

    public void SetValid()
    {
        Modulate = Colors.White;
        spriteRoot.Modulate = Modulate;
    }

    public void SetDimensions(Vector2I dimensions)
    {
        topRight.Position = dimensions * new Vector2I(64, 0);
        bottomLeft.Position = dimensions * new Vector2I(0, 64);
        bottomRight.Position = dimensions * new Vector2I(64, 64);
    }

    public void AddSpriteNode(Sprite2D spriteNode)
    {
        upDownRoot.AddChild(spriteNode);
    }

    public void DoHoverAnimation()
    {
        if (spriteTween != null && spriteTween.IsValid())
        {
            spriteTween.Kill();
        }

        spriteTween = CreateTween();
        spriteTween
            .TweenProperty(spriteRoot, "global_position", GlobalPosition, .3)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);
    }
}
