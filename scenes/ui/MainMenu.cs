using Game.Autoload;
using Godot;

namespace Game.Ui;

public partial class MainMenu : Node
{
    private Button playButton;
    private Button optionsButton;
    private Button quitButton;

    public override void _Ready()
    {
        playButton = GetNode<Button>("%PlayButton");
        optionsButton = GetNode<Button>("%OptionsButton");
        quitButton = GetNode<Button>("%QuitButton");

        playButton.Pressed += OnPlayButtonPressed;
    }

    private void OnPlayButtonPressed()
    {
        LevelManager.Instance.ChangeToLevel(0);
    }
}
