using Godot;

namespace Game.Ui;

public partial class ResourceIndicator : Node2D
{
    private AnimatedSprite2D animatedSprite2D;
    private Tween activeTween;

    public override void _Ready()
    {
        animatedSprite2D = GetNode<AnimatedSprite2D>("%AnimatedSprite2D");

        var duration = GD.RandRange(.5, .55);

        activeTween = CreateTween();
        activeTween.SetLoops();
        activeTween
            .TweenProperty(animatedSprite2D, "position", Vector2.Up * 4, duration)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.InOut);
        activeTween
            .TweenProperty(animatedSprite2D, "position", Vector2.Down * 4, duration)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.InOut);
    }

    public void Destroy()
    {
        if (activeTween != null && activeTween.IsValid())
        {
            activeTween.Kill();
        }

        activeTween = CreateTween();
        activeTween.SetParallel();
        activeTween.TweenInterval(GD.RandRange(.1, .3));
        activeTween.Chain();
        activeTween
            .TweenProperty(animatedSprite2D, "scale", Vector2.Zero, .3)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.In);
        activeTween
            .TweenProperty(animatedSprite2D, "position", Vector2.Up * 32, .3)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.In);
        activeTween.Chain();
        activeTween.TweenCallback(Callable.From(() => QueueFree()));
    }
}
