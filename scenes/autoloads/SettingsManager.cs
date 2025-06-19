using Godot;

namespace Game.Autoload;

public partial class SettingsManager : Node
{
    public static SettingsManager Instance { get; private set; }

    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated)
        {
            Instance = this;
        }
    }

    public override void _Ready()
    {
        RenderingServer.SetDefaultClearColor(new Color("47aba9"));
        GetViewport().GetWindow().MinSize = new Vector2I(1280, 720);
    }
}
