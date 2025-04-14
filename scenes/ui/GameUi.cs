using Game.Manager;
using Game.Resources.Building;
using Game.UI;
using Godot;

namespace Game.Ui;

public partial class GameUi : CanvasLayer
{
    [Signal]
    public delegate void BuildingResourceSelectedEventHandler(BuildingResource buildingResource);

    [Export]
    private BuildingManager buildingManager;

    [Export]
    private BuildingResource[] buildingResources;

    [Export]
    private PackedScene buildingSectionScene;

    private VBoxContainer buildingSectionContainer;
    private Label resourceLabel;

    public override void _Ready()
    {
        buildingSectionContainer = GetNode<VBoxContainer>("%BuildingSectionContainer");
        resourceLabel = GetNode<Label>("%ResourceLabel");

        CreateBuildingSections();

        buildingManager.AvailableResourceCountChanged += OnAvailableResourceCountChanged;
    }

    private void CreateBuildingSections()
    {
        foreach (var buildingResource in buildingResources)
        {
            var buildingSection = buildingSectionScene.Instantiate<BuildingSection>();
            buildingSectionContainer.AddChild(buildingSection);

            buildingSection.SetBuildingResource(buildingResource);

            buildingSection.SelectButtonPressed += () =>
            {
                EmitSignal(SignalName.BuildingResourceSelected, buildingResource);
            };
        }
    }

    private void OnAvailableResourceCountChanged(int availableResourceCount)
    {
        resourceLabel.Text = availableResourceCount.ToString();
    }
}
