using Game.Autoload;
using Godot;

namespace Game.Ui;

public partial class OptionsMenu : CanvasLayer
{
    private const string SFX_BUS_NAME = "SFX";
    private const string MUSIC_BUS_NAME = "Music";

    [Signal]
    public delegate void DonePressedEventHandler();

    private Button sfxDownButton;
    private Button sfxUpButton;
    private Label sfxLabel;
    private Button musicDownButton;
    private Button musicUpButton;
    private Label musicLabel;
    private Button windowButton;
    private Button doneButton;

    public override void _Ready()
    {
        sfxDownButton = GetNode<Button>("%SFXDownButton");
        sfxUpButton = GetNode<Button>("%SFXUpButton");
        sfxLabel = GetNode<Label>("%SFXLabel");
        musicDownButton = GetNode<Button>("%MusicDownButton");
        musicUpButton = GetNode<Button>("%MusicUpButton");
        musicLabel = GetNode<Label>("%MusicLabel");
        windowButton = GetNode<Button>("%WindowButton");
        doneButton = GetNode<Button>("%DoneButton");

        AudioHelpers.RegisterButtons(
            new Button[]
            {
                sfxDownButton,
                sfxUpButton,
                musicDownButton,
                musicUpButton,
                windowButton,
                doneButton,
            }
        );

        UpdateDisplay();

        sfxDownButton.Pressed += () =>
        {
            ChangeBusVolume(SFX_BUS_NAME, -.1f);
        };

        sfxUpButton.Pressed += () =>
        {
            ChangeBusVolume(SFX_BUS_NAME, .1f);
        };

        musicDownButton.Pressed += () =>
        {
            ChangeBusVolume(MUSIC_BUS_NAME, -.1f);
        };

        musicUpButton.Pressed += () =>
        {
            ChangeBusVolume(MUSIC_BUS_NAME, .1f);
        };

        windowButton.Pressed += OnWindowButtonPressed;

        doneButton.Pressed += OnDoneButtonPressed;
    }

    private void UpdateDisplay()
    {
        sfxLabel.Text = Mathf
            .Round(OptionsHelpers.GetBusVolumePercent(SFX_BUS_NAME) * 10)
            .ToString();

        musicLabel.Text = Mathf
            .Round(OptionsHelpers.GetBusVolumePercent(MUSIC_BUS_NAME) * 10)
            .ToString();

        windowButton.Text = OptionsHelpers.IsFullscreen() ? "Fullscreen" : "Windowed";
    }

    private void ChangeBusVolume(string busName, float change)
    {
        var busVolumePercent = OptionsHelpers.GetBusVolumePercent(busName);
        busVolumePercent = Mathf.Clamp(busVolumePercent + change, 0, 1);
        OptionsHelpers.SetBusVolumePercent(busName, busVolumePercent);
        UpdateDisplay();
    }

    private void OnWindowButtonPressed()
    {
        OptionsHelpers.ToggleWindowMode();
        UpdateDisplay();
    }

    private void OnDoneButtonPressed()
    {
        EmitSignal(SignalName.DonePressed);
    }
}
