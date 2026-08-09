using System;
using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace WiiMotePlusDemo;

internal static class Program
{
    private const int ScreenWidth = 1100;
    private const int ScreenHeight = 720;
    private const float RadToDeg = 180.0f / MathF.PI;

    [STAThread]
    private static void Main()
    {
        SetConfigFlags(ConfigFlags.Msaa4xHint | ConfigFlags.VSyncHint);
        InitWindow(ScreenWidth, ScreenHeight, "Wii MotionPlus - raylib demo");
        SetTargetFPS(60);

        Camera3D camera = new()
        {
            Position = new Vector3(0.0f, 2.2f, 8.0f),
            Target = Vector3.Zero,
            Up = Vector3.UnitY,
            FovY = 42.0f,
            Projection = CameraProjection.Perspective
        };

        string modelPath = System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", "wiimote.glb");
        Model model = LoadModel(modelPath);
        BoundingBox bounds = GetModelBoundingBox(model);
        Vector3 modelCenter = (bounds.Min + bounds.Max) * 0.5f;
        Vector3 modelSize = bounds.Max - bounds.Min;
        float largestDimension = MathF.Max(modelSize.X, MathF.Max(modelSize.Y, modelSize.Z));
        float modelScale = largestDimension > 0.0f ? 4.8f / largestDimension : 1.0f;

        using WiimoteConnection remote = new();
        remote.TryConnect();

        float pitch = 0.0f;
        float roll = 0.0f;
        float yaw = 0.0f;

        while (!WindowShouldClose())
        {
            if (!float.IsFinite(pitch)) pitch = 0.0f;
            if (!float.IsFinite(roll)) roll = 0.0f;
            if (!float.IsFinite(yaw)) yaw = 0.0f;

            if (IsKeyPressed(KeyboardKey.R) && !remote.IsConnected)
                remote.TryConnect();
            if (IsKeyPressed(KeyboardKey.C) && remote.IsConnected)
            {
                yaw = 0.0f;
                remote.StartCalibration();
            }

            float dt = MathF.Min(GetFrameTime(), 0.05f);
            if (remote.TryGetMotion(out MotionSample motion))
            {
                Vector3 acceleration = motion.Acceleration;
                if (IsFinite(acceleration))
                {
                    float targetRoll = MathF.Atan2(-acceleration.X, acceleration.Z) * RadToDeg;
                    float targetPitch = MathF.Atan2(
                        -acceleration.Y,
                        MathF.Sqrt(acceleration.X * acceleration.X + acceleration.Z * acceleration.Z)) * RadToDeg;

                    float blend = 1.0f - MathF.Exp(-10.0f * dt);
                    roll = LerpAngle(roll, targetRoll, blend);
                    pitch = LerpAngle(pitch, targetPitch, blend);
                }

                if (remote.IsCalibrated && IsFinite(motion.GyroDegreesPerSecond))
                    yaw = WrapAngle(yaw - motion.GyroDegreesPerSecond.X * dt);
            }

            SetWindowTitle($"Wii MotionPlus - raylib demo | {remote.Status}");

            BeginDrawing();
            ClearBackground(new Color(18, 22, 30, 255));
            BeginMode3D(camera);
            DrawGrid(20, 0.5f);

            Rlgl.PushMatrix();
            Rlgl.Rotatef(yaw, 0.0f, 1.0f, 0.0f);
            Rlgl.Rotatef(roll, 0.0f, 0.0f, 1.0f);
            Rlgl.Rotatef(pitch, 1.0f, 0.0f, 0.0f);
            // GLB local +X is its button face; rotate it toward world-up.
            Rlgl.Rotatef(90.0f, 0.0f, 0.0f, 1.0f);
            Rlgl.Scalef(modelScale, modelScale, modelScale);
            Rlgl.Translatef(-modelCenter.X, -modelCenter.Y, -modelCenter.Z);
            DrawModel(model, Vector3.Zero, 1.0f, Color.White);
            Rlgl.PopMatrix();

            EndMode3D();
            DrawText("Wii Remote Plus orientation", 28, 24, 30, Color.RayWhite);
            DrawText(remote.Status, 30, 66, 20, remote.IsConnected ? Color.Lime : Color.Orange);
            DrawText($"Pitch {pitch,7:0.0} deg    Roll {roll,7:0.0} deg    Yaw {yaw,7:0.0} deg", 30, 98, 20, Color.LightGray);
            DrawText("C: center + calibrate gyro     R: reconnect     Esc: quit", 30, ScreenHeight - 42, 18, Color.Gray);
            DrawFPS(ScreenWidth - 100, 20);
            EndDrawing();
        }

        UnloadModel(model);
        CloseWindow();
    }

    private static float LerpAngle(float current, float target, float amount)
    {
        float delta = WrapAngle(target - current);
        return current + delta * amount;
    }

    private static bool IsFinite(Vector3 value) =>
        float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Z);

    private static float WrapAngle(float angle)
    {
        while (angle > 180.0f) angle -= 360.0f;
        while (angle < -180.0f) angle += 360.0f;
        return angle;
    }
}
