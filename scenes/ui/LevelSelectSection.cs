using Game.Autoload;
using Game.Resources.Level;
using Godot;

namespace Game.Ui;

public partial class LevelSelectSection : PanelContainer
{
    [Signal]
    public delegate void LevelSelectedEventHandler(int levelIndex);

    private Label levelNumberLabel;
    private Label resourceCountLabel;
    private Button button;
    private TextureRect completedIndicator;
    private int levelIndex;

    public override void _Ready()
    {
        levelNumberLabel = GetNode<Label>("%LevelNumberLabel");
        resourceCountLabel = GetNode<Label>("%ResourceCountLabel");
        button = GetNode<Button>("%Button");
        completedIndicator = GetNode<TextureRect>("%CompletedIndicator");

        AudioHelpers.RegisterButtons(new Button[] { button });

        button.Pressed += OnButtonPressed;
    }

    public void SetLevelIndex(int index)
    {
        levelIndex = index;
        levelNumberLabel.Text = $"level {index + 1}";
    }

    public void SetLevelDefinition(LevelDefinitionResource levelDefinitionResource)
    {
        resourceCountLabel.Text = levelDefinitionResource.StartingResourceCount.ToString();
        completedIndicator.Visible = SaveManager.IsLevelCompleted(levelDefinitionResource.Id);
    }

    private void OnButtonPressed()
    {
        EmitSignal(SignalName.LevelSelected, levelIndex);
    }
}
