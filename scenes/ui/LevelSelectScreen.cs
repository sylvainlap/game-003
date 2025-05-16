using Game.Autoload;
using Godot;

namespace Game.Ui;

public partial class LevelSelectScreen : MarginContainer
{
    [Signal]
    public delegate void BackButtonPressedEventHandler();

    [Export]
    private PackedScene levelSelectSectionScene;

    private GridContainer gridContainer;
    private Button backButton;

    public override void _Ready()
    {
        gridContainer = GetNode<GridContainer>("%GridContainer");
        backButton = GetNode<Button>("BackButton");

        var levelDefinitions = LevelManager.GetLevelDefinitionResources();

        for (var i = 0; i < levelDefinitions.Length; i++)
        {
            var levelDefinition = levelDefinitions[i];

            var levelSelectSection = levelSelectSectionScene.Instantiate<LevelSelectSection>();
            gridContainer.AddChild(levelSelectSection);

            levelSelectSection.SetLevelIndex(i);
            levelSelectSection.SetLevelDefinition(levelDefinition);
            levelSelectSection.LevelSelected += OnLevelSelected;
        }

        backButton.Pressed += OnBackButtonPressed;
    }

    private void OnLevelSelected(int levelIndex)
    {
        LevelManager.Instance.ChangeToLevel(levelIndex);
    }

    private void OnBackButtonPressed()
    {
        EmitSignal(SignalName.BackButtonPressed);
    }
}
