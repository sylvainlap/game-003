using System.Collections.Generic;
using Godot;

namespace Game.Autoload;

public partial class AudioHelpers : Node
{
    public static AudioHelpers Instance { get; private set; }

    private AudioStreamPlayer explosionAudioStreamPlayer;
    private AudioStreamPlayer clickAudioStreamPlayer;
    private AudioStreamPlayer victoryAudioStreamPlayer;
    private AudioStreamPlayer musicAudioStreamPlayer;

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
        clickAudioStreamPlayer = GetNode<AudioStreamPlayer>("ClickAudioStreamPlayer");
        victoryAudioStreamPlayer = GetNode<AudioStreamPlayer>("VictoryAudioStreamPlayer");
        musicAudioStreamPlayer = GetNode<AudioStreamPlayer>("MusicAudioStreamPlayer");

        musicAudioStreamPlayer.Finished += OnMusicFinished;
    }

    public static void PlayBuildingDestruction()
    {
        Instance.explosionAudioStreamPlayer.Play();
    }

    public static void PlayVictory()
    {
        Instance.victoryAudioStreamPlayer.Play();
    }

    public static void RegisterButtons(IEnumerable<Button> buttons)
    {
        foreach (var button in buttons)
        {
            button.Pressed += Instance.OnButtonPressed;
        }
    }

    private void OnButtonPressed()
    {
        clickAudioStreamPlayer.Play();
    }

    private void OnMusicFinished()
    {
        GetTree().CreateTimer(5).Timeout += OnMusicDelayTimerTimeout;
    }

    private void OnMusicDelayTimerTimeout()
    {
        musicAudioStreamPlayer.Play();
    }
}
