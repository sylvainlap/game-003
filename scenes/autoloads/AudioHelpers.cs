using Godot;

namespace Game.Autoload;

public partial class AudioHelpers : Node
{
    public static AudioHelpers Instance { get; private set; }

    private AudioStreamPlayer explosionAudioStreamPlayer;

    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated)
        {
            Instance = this;
        }
    }

    public override void _Ready()
    {
        explosionAudioStreamPlayer = GetNode<AudioStreamPlayer>("ExplosionAudioStreamPlayer");
    }

    public static void PlayBuildingDestruction()
    {
        Instance.explosionAudioStreamPlayer.Play();
    }
}
