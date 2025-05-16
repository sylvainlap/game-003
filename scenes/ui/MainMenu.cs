using Godot;

namespace Game.Ui;

public partial class MainMenu : Node
{
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

        playButton.Pressed += OnPlayButtonPressed;
        quitButton.Pressed += OnQuitButtonPressed;
        levelSelectScreen.BackButtonPressed += OnLevelSelectScreenBackButtonPressed;
    }

    private void OnPlayButtonPressed()
    {
        mainMenuContainer.Visible = false;
        levelSelectScreen.Visible = true;
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
