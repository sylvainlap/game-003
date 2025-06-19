using Godot;

namespace Game;

public partial class GameCamera : Camera2D
{
    private const float PAN_SPEED = 500;
    private const int TILE_SIZE = 64;
    private const float NOISE_SAMPLE_GROWTH = .1f;
    private const float MAX_CAMERA_OFFSET = 24f;
    private const float NOISE_FREQUENCY_MULTIPLIER = 100f;
    private const float SHAKE_DECAY = 2f;

    [Export]
    private FastNoiseLite shakeNoise;

    private readonly StringName ACTION_PAN_UP = "pan_up";
    private readonly StringName ACTION_PAN_DOWN = "pan_down";
    private readonly StringName ACTION_PAN_LEFT = "pan_left";
    private readonly StringName ACTION_PAN_RIGHT = "pan_right";

    private static GameCamera Instance;

    private Vector2 noiseSample;
    private float currentShakePercentage;

    public static void Shake()
    {
        Instance.currentShakePercentage = 1;
    }

    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated)
        {
            Instance = this;
        }
    }

    public override void _Process(double delta)
    {
        var movementVector = Input.GetVector(
            ACTION_PAN_LEFT,
            ACTION_PAN_RIGHT,
            ACTION_PAN_UP,
            ACTION_PAN_DOWN
        );

        GlobalPosition += movementVector * PAN_SPEED * (float)delta;

        var viewportRect = GetViewportRect();
        var halfWidth = viewportRect.Size.X / 2;
        var halfHeight = viewportRect.Size.Y / 2;
        var xClamped = Mathf.Clamp(GlobalPosition.X, LimitLeft + halfWidth, LimitRight - halfWidth);
        var yClamped = Mathf.Clamp(
            GlobalPosition.Y,
            LimitTop + halfHeight,
            LimitBottom - halfHeight
        );

        GlobalPosition = new Vector2(xClamped, yClamped);

        ApplyCameraShake(delta);
    }

    public void SetBoundingRect(Rect2I boundingRect)
    {
        LimitLeft = boundingRect.Position.X * TILE_SIZE;
        LimitRight = boundingRect.End.X * TILE_SIZE;
        LimitTop = boundingRect.Position.Y * TILE_SIZE;
        LimitBottom = boundingRect.End.Y * TILE_SIZE;
    }

    public void CenterOnPosition(Vector2 position)
    {
        GlobalPosition = position;
    }

    private void ApplyCameraShake(double delta)
    {
        if (currentShakePercentage > 0)
        {
            noiseSample.X += NOISE_SAMPLE_GROWTH * NOISE_FREQUENCY_MULTIPLIER * (float)delta;
            noiseSample.Y += NOISE_SAMPLE_GROWTH * NOISE_FREQUENCY_MULTIPLIER * (float)delta;

            currentShakePercentage = Mathf.Clamp(
                currentShakePercentage - (SHAKE_DECAY * (float)delta),
                0,
                1
            );
        }

        var xSample = shakeNoise.GetNoise2D(noiseSample.X, 0);
        var ySample = shakeNoise.GetNoise2D(0, noiseSample.Y);

        Offset =
            new Vector2(MAX_CAMERA_OFFSET * xSample, MAX_CAMERA_OFFSET * ySample)
            * currentShakePercentage;
    }
}
