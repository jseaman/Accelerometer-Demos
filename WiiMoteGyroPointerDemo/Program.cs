using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace WiiMoteGyroPointerDemo;

internal static class Program
{
    private const int WindowWidth = 1100;
    private const int WindowHeight = 830;
    private const float RadToDeg = 180.0f / MathF.PI;
    private const float HorizontalHalfAngle = 35.0f;
    private const float VerticalHalfAngle = 22.0f;
    private const float YawDeadZone = 0.40f;
    private const float RollDeadZone = 0.40f;
    private const float PitchDeadZone = 0.50f;
    private const int GyroAverageSamples = 8;
    private const int MaximumPenSegments = 30000;
    private static readonly Rectangle ScreenArea = new(70, 120, 960, 540);

    [STAThread]
    private static void Main()
    {
        SetConfigFlags(ConfigFlags.Msaa4xHint | ConfigFlags.VSyncHint);
        InitWindow(WindowWidth, WindowHeight, "Wii Remote MotionPlus screen pointer");
        SetTargetFPS(60);

        using WiimoteConnection remote = new();
        remote.TryConnect();

        float horizontalAngle = 0.0f;
        float verticalAngle = 0.0f;
        float roll = 0.0f;
        float accelerationPitchCenter = 0.0f;
        bool havePitchCenter = false;
        MotionSample displayed = default;
        Vector3 filteredGyro = Vector3.Zero;
        RollingVectorAverage gyroAverage = new(GyroAverageSamples);
        bool useGyroAverage = true;
        bool penDown = false;
        Vector2? previousPenPosition = null;
        Queue<PenSegment> penSegments = new();

        while (!WindowShouldClose())
        {
            if (IsKeyPressed(KeyboardKey.R) && !remote.IsConnected)
                remote.TryConnect();
            if (IsKeyPressed(KeyboardKey.A))
            {
                useGyroAverage = !useGyroAverage;
                gyroAverage.Reset();
            }
            if (IsKeyPressed(KeyboardKey.C) && remote.IsConnected)
            {
                horizontalAngle = 0.0f;
                verticalAngle = 0.0f;
                havePitchCenter = false;
                filteredGyro = Vector3.Zero;
                gyroAverage.Reset();
                remote.StartCalibration();
            }

            float dt = MathF.Min(GetFrameTime(), 0.05f);
            if (remote.TryGetMotion(out MotionSample motion))
            {
                penDown = motion.BPressed;
                if (IsFinite(motion.Acceleration) && IsFinite(motion.RawGyroscope))
                    displayed = motion;

                Vector3 acceleration = motion.Acceleration;
                float magnitude = acceleration.Length();
                bool gravityValid = IsFinite(acceleration) && float.IsFinite(magnitude) && magnitude > 0.0001f;

                if (gravityValid)
                {
                    float targetRoll = MathF.Atan2(-acceleration.X, acceleration.Z) * RadToDeg;
                    roll = LerpAngle(roll, targetRoll, 1.0f - MathF.Exp(-12.0f * dt));
                }

                if (remote.IsCalibrated && IsFinite(motion.GyroDegreesPerSecond))
                {
                    Vector3 deadZonedGyro = new(
                        ApplySoftDeadZone(motion.GyroDegreesPerSecond.X, YawDeadZone),
                        ApplySoftDeadZone(motion.GyroDegreesPerSecond.Y, RollDeadZone),
                        ApplySoftDeadZone(motion.GyroDegreesPerSecond.Z, PitchDeadZone));
                    filteredGyro = useGyroAverage
                        ? gyroAverage.Add(deadZonedGyro)
                        : deadZonedGyro;

                    // WiimoteLib axes: X=yaw, Y=roll, Z=pitch. Undo the
                    // controller's accelerometer-derived roll to obtain the
                    // screen-horizontal rate from body yaw and pitch.
                    float rollRadians = roll / RadToDeg;
                    float cosine = MathF.Cos(rollRadians);
                    float sine = MathF.Sin(rollRadians);
                    float yawRate = filteredGyro.X;
                    float pitchRate = filteredGyro.Z;
                    float screenHorizontalRate = yawRate * cosine - pitchRate * sine;

                    // Clamp the integrated state itself, not just its screen
                    // projection. This prevents off-screen gyro windup.
                    horizontalAngle = Math.Clamp(
                        horizontalAngle + screenHorizontalRate * dt,
                        -HorizontalHalfAngle,
                        HorizontalHalfAngle);

                    // Use gravity as the absolute vertical pointer reference.
                    // Gyro pitch is deliberately not integrated into screen Y.
                    if (gravityValid)
                    {
                        float accelerationPitch = MathF.Atan2(
                            -acceleration.Y,
                            MathF.Sqrt(acceleration.X * acceleration.X + acceleration.Z * acceleration.Z)) * RadToDeg;
                        if (!havePitchCenter)
                        {
                            accelerationPitchCenter = accelerationPitch;
                            havePitchCenter = true;
                        }

                        float confidence = Math.Clamp(1.0f - MathF.Abs(magnitude - 1.0f) / 0.30f, 0.0f, 1.0f);
                        float targetVertical = WrapAngle(accelerationPitchCenter - accelerationPitch);
                        verticalAngle = LerpAngle(verticalAngle, targetVertical,
                            (1.0f - MathF.Exp(-10.0f * dt)) * confidence);
                    }
                }
            }

            if (!float.IsFinite(horizontalAngle)) horizontalAngle = 0.0f;
            if (!float.IsFinite(verticalAngle)) verticalAngle = 0.0f;
            if (!float.IsFinite(roll)) roll = 0.0f;

            Vector2 pointer = ProjectPointer(horizontalAngle, verticalAngle);
            if (penDown)
            {
                if (previousPenPosition is Vector2 previous && Vector2.DistanceSquared(previous, pointer) >= 0.25f)
                {
                    penSegments.Enqueue(new PenSegment(previous, pointer));
                    if (penSegments.Count > MaximumPenSegments)
                        penSegments.Dequeue();
                }
                previousPenPosition = pointer;
            }
            else
            {
                previousPenPosition = null;
            }

            BeginDrawing();
            ClearBackground(new Color(12, 16, 24, 255));
            DrawText("MotionPlus screen pointer (no IR)", 70, 24, 30, Color.RayWhite);
            DrawText(remote.Status, 70, 66, 19, remote.IsConnected ? Color.Lime : Color.Orange);
            DrawVirtualScreen();
            foreach (PenSegment segment in penSegments)
                DrawLineEx(segment.From, segment.To, 4.0f, new Color(80, 220, 255, 255));
            DrawPointer(pointer, roll);
            DrawTelemetry(remote, displayed, filteredGyro, horizontalAngle, verticalAngle, roll, useGyroAverage);
            EndDrawing();
        }

        CloseWindow();
    }

    private static Vector2 ProjectPointer(float horizontalAngle, float verticalAngle)
    {
        float x = Math.Clamp(horizontalAngle / HorizontalHalfAngle, -1.0f, 1.0f);
        float y = Math.Clamp(verticalAngle / VerticalHalfAngle, -1.0f, 1.0f);
        return new Vector2(
            ScreenArea.X + ScreenArea.Width * (0.5f + x * 0.5f),
            ScreenArea.Y + ScreenArea.Height * (0.5f + y * 0.5f));
    }

    private static void DrawVirtualScreen()
    {
        DrawRectangleRounded(new Rectangle(48, 98, 1004, 584), 0.025f, 8, new Color(42, 48, 60, 255));
        DrawRectangleRec(ScreenArea, new Color(7, 13, 23, 255));
        for (int x = 1; x < 8; x++)
            DrawLine((int)(ScreenArea.X + ScreenArea.Width * x / 8), (int)ScreenArea.Y,
                (int)(ScreenArea.X + ScreenArea.Width * x / 8), (int)(ScreenArea.Y + ScreenArea.Height),
                new Color(24, 42, 62, 255));
        for (int y = 1; y < 6; y++)
            DrawLine((int)ScreenArea.X, (int)(ScreenArea.Y + ScreenArea.Height * y / 6),
                (int)(ScreenArea.X + ScreenArea.Width), (int)(ScreenArea.Y + ScreenArea.Height * y / 6),
                new Color(24, 42, 62, 255));
        Vector2 center = new(ScreenArea.X + ScreenArea.Width / 2, ScreenArea.Y + ScreenArea.Height / 2);
        DrawCircleLines((int)center.X, (int)center.Y, 18, new Color(80, 100, 125, 255));
        DrawLine((int)center.X - 28, (int)center.Y, (int)center.X + 28, (int)center.Y, new Color(80, 100, 125, 255));
        DrawLine((int)center.X, (int)center.Y - 28, (int)center.X, (int)center.Y + 28, new Color(80, 100, 125, 255));
        DrawRectangleLinesEx(ScreenArea, 2.0f, new Color(125, 155, 190, 255));
    }

    private static void DrawPointer(Vector2 center, float rollDegrees)
    {
        Vector2 tip = Rotate(new Vector2(0, -25), rollDegrees) + center;
        Vector2 left = Rotate(new Vector2(-12, 14), rollDegrees) + center;
        Vector2 right = Rotate(new Vector2(12, 14), rollDegrees) + center;
        DrawCircleV(center, 25, Fade(Color.Gold, 0.16f));
        DrawTriangle(tip, left, right, Color.Gold);
        DrawTriangleLines(tip, left, right, Color.Black);
        DrawCircleV(center, 3, Color.RayWhite);
    }

    private static void DrawTelemetry(
        WiimoteConnection remote, MotionSample sample, Vector3 filteredGyro,
        float horizontalAngle, float verticalAngle, float roll, bool useGyroAverage)
    {
        string calibration = remote.IsCalibrated
            ? "READY"
            : $"KEEP STILL {remote.CalibrationSamples}/{remote.CalibrationTarget}";
        DrawText($"Accel g  X {sample.Acceleration.X,7:0.000}   Y {sample.Acceleration.Y,7:0.000}   Z {sample.Acceleration.Z,7:0.000}", 70, 700, 17, Color.SkyBlue);
        DrawText($"Gyro raw X {sample.RawGyroscope.X,6:0}   Y {sample.RawGyroscope.Y,6:0}   Z {sample.RawGyroscope.Z,6:0}", 585, 700, 17, Color.LightGray);
        DrawText($"Gyro deg/s yaw {sample.GyroDegreesPerSecond.X,7:0.00}   roll {sample.GyroDegreesPerSecond.Y,7:0.00}   pitch {sample.GyroDegreesPerSecond.Z,7:0.00}", 70, 726, 17, Color.Lime);
        DrawText($"Filtered   yaw {filteredGyro.X,7:0.00}   roll {filteredGyro.Y,7:0.00}   pitch {filteredGyro.Z,7:0.00}", 585, 726, 17, Color.SkyBlue);
        DrawText($"Pointer yaw/gyro {horizontalAngle,7:0.0} deg   pitch/gravity {verticalAngle,7:0.0} deg   gravity roll {roll,7:0.0} deg", 70, 752, 17, Color.Gold);
        string average = useGyroAverage ? $"ON ({GyroAverageSamples} samples)" : "OFF";
        DrawText($"C: center + recalibrate    A: gyro average {average}    R: reconnect    Esc: quit    Calibration: {calibration}", 70, 790, 17, remote.IsCalibrated ? Color.LightGray : Color.Orange);
    }

    private static Vector2 Rotate(Vector2 vector, float degrees)
    {
        float radians = degrees / RadToDeg;
        float cosine = MathF.Cos(radians);
        float sine = MathF.Sin(radians);
        return new Vector2(vector.X * cosine - vector.Y * sine, vector.X * sine + vector.Y * cosine);
    }

    private static float LerpAngle(float current, float target, float amount) =>
        current + WrapAngle(target - current) * amount;

    private static float ApplySoftDeadZone(float value, float threshold)
    {
        float magnitude = MathF.Abs(value);
        if (magnitude <= threshold)
            return 0.0f;

        return MathF.CopySign(magnitude - threshold, value);
    }

    private static float WrapAngle(float angle)
    {
        while (angle > 180.0f) angle -= 360.0f;
        while (angle < -180.0f) angle += 360.0f;
        return angle;
    }

    private static bool IsFinite(Vector3 value) =>
        float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Z);

    private sealed class RollingVectorAverage
    {
        private readonly Vector3[] samples;
        private Vector3 sum;
        private int next;
        private int count;

        public RollingVectorAverage(int capacity) => samples = new Vector3[capacity];

        public Vector3 Add(Vector3 sample)
        {
            if (count == samples.Length)
                sum -= samples[next];
            else
                count++;

            samples[next] = sample;
            sum += sample;
            next = (next + 1) % samples.Length;
            return sum / count;
        }

        public void Reset()
        {
            Array.Clear(samples);
            sum = Vector3.Zero;
            next = 0;
            count = 0;
        }
    }

    private readonly record struct PenSegment(Vector2 From, Vector2 To);
}
