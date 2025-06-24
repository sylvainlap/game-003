using Game.Autoload;
using Godot;

namespace Game.Ui;

public partial class MainMenu : Node
{
    [Export]
    private PackedScene optionsMenuScene;

    private Button playButton;
    private Button optionsButton;
    private Button quitButton;
    private MarginContainer mainMenuContainer;
    private LevelSelectScreen levelSelectScreen;

    public override void _Ready()
    {
        playButton = GetNode<Button>("%PlayButton");
        optionsButton = GetNode<Button>("%OptionsButton");
        quitButton = GetNode<Button>("%QuitButton");
        mainMenuContainer = GetNode<MarginContainer>("%MainMenuContainer");
        levelSelectScreen = GetNode<LevelSelectScreen>("%LevelSelectScreen");

        mainMenuContainer.Visible = true;
        levelSelectScreen.Visible = false;

        AudioHelpers.RegisterButtons(new Button[] { playButton, optionsButton, quitButton });

        playButton.Pressed += OnPlayButtonPressed;
        optionsButton.Pressed += OnOptionsButtonPressed;
        quitButton.Pressed += OnQuitButtonPressed;
        levelSelectScreen.BackButtonPressed += OnLevelSelectScreenBackButtonPressed;
    }

    private void OnPlayButtonPressed()
    {
        mainMenuContainer.Visible = false;
        levelSelectScreen.Visible = true;
    }

    private void OnOptionsButtonPressed()
    {
        mainMenuContainer.Visible = false;

        var optionsMenu = optionsMenuScene.Instantiate<OptionsMenu>();
        AddChild(optionsMenu);
        optionsMenu.DonePressed += () =>
        {
            OnOptionsDonePressed(optionsMenu);
        };
    }

    private void OnOptionsDonePressed(OptionsMenu optionsMenu)
    {
        optionsMenu.QueueFree();
        mainMenuContainer.Visible = true;
    }

    private void OnQuitButtonPressed()
    {
        GetTree().Quit();
    }

    private void OnLevelSelectScreenBackButtonPressed()
    {
        mainMenuContainer.Visible = true;
        levelSelectScreen.Visible = false;
    }
}
