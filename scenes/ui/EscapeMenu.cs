using Game.Autoload;
using Game.Ui;
using Godot;

public partial class EscapeMenu : CanvasLayer
{
    private readonly StringName ACTION_ESCAPE = "escape";

    [Export(PropertyHint.File, "*.tscn")]
    private string mainMenuScenePath;

    [Export]
    private PackedScene optionsMenuScene;

    private MarginContainer marginContainer;
    private Button resumeButton;
    private Button optionsButton;
    private Button quitButton;

    public override void _Ready()
    {
        marginContainer = GetNode<MarginContainer>("MarginContainer");
        resumeButton = GetNode<Button>("%ResumeButton");
        optionsButton = GetNode<Button>("%OptionsButton");
        quitButton = GetNode<Button>("%QuitButton");

        AudioHelpers.RegisterButtons(new Button[] { resumeButton, optionsButton, quitButton });

        resumeButton.Pressed += OnResumeButtonPressed;
        optionsButton.Pressed += OnOptionsButtonPressed;
        quitButton.Pressed += OnQuitButtonPressed;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed(ACTION_ESCAPE))
        {
            QueueFree();
            GetViewport().SetInputAsHandled();
        }
    }

    private void OnResumeButtonPressed()
    {
        QueueFree();
    }

    private void OnOptionsButtonPressed()
    {
        marginContainer.Visible = false;
        var optionsMenu = optionsMenuScene.Instantiate<OptionsMenu>();
        AddChild(optionsMenu);
        optionsMenu.DonePressed += () =>
        {
            OnOptionsDonePressed(optionsMenu);
        };
    }

    private void OnQuitButtonPressed()
    {
        GetTree().ChangeSceneToFile(mainMenuScenePath);
    }

    private void OnOptionsDonePressed(OptionsMenu optionsMenu)
    {
        marginContainer.Visible = true;
        optionsMenu.QueueFree();
    }
}
