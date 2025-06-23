using Game.Ui;
using Godot;

namespace Game.Manager;

public partial class FloatingTextManager : Node
{
    [Export]
    private PackedScene floatingTextScene;

    public static FloatingTextManager Instance { get; private set; }

    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated)
        {
            Instance = this;
        }
    }

    public static void ShowMessage(string message)
    {
        var floatingText = Instance.floatingTextScene.Instantiate<FloatingText>();
        Instance.AddChild(floatingText);
        floatingText.SetText(message);
        floatingText.GlobalPosition = floatingText.GetGlobalMousePosition();
    }
}
