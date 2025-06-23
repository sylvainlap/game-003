using Game.Component;
using Godot;

namespace Game.Building;

public partial class GoblinCamp : Node2D
{
    [Export]
    private BuildingComponent buildingComponent;

    [Export]
    private AnimatedSprite2D animatedSprite2D;

    [Export]
    private AnimatedSprite2D fireAnimatedSprite2D;

    private AudioStreamPlayer audioStreamPlayer;

    public override void _Ready()
    {
        audioStreamPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");

        fireAnimatedSprite2D.Visible = false;

        buildingComponent.Disabled += OnDisabled;
        buildingComponent.Enabled += OnEnabled;
    }

    public void OnDisabled()
    {
        audioStreamPlayer.Play();
        animatedSprite2D.Play("destroyed");
        fireAnimatedSprite2D.Visible = true;
    }

    public void OnEnabled()
    {
        animatedSprite2D.Play("default");
        fireAnimatedSprite2D.Visible = false;
    }
}
