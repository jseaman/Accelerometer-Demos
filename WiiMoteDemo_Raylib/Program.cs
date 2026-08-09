using System;
using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace WiiMoteDemo_Raylib;

internal static class Program
{
    private enum DemoMode
    {
        Steering,
        FlatOrientation
    }

    private const int ScreenWidth = 1100;
    private const int ScreenHeight = 720;
    private const float RadToDeg = 180.0f / MathF.PI;

    [STAThread]
    private static void Main()
    {
        SetConfigFlags(ConfigFlags.Msaa4xHint | ConfigFlags.VSyncHint);
        InitWindow(ScreenWidth, ScreenHeight, "Wii Remote gravity steering");
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

        float steering = 0.0f;
        float steeringCenter = 0.0f;
        bool steeringCentered = false;
        float pitch = 0.0f;
        float roll = 0.0f;
        DemoMode mode = DemoMode.Steering;

        while (!WindowShouldClose())
        {
            if (!float.IsFinite(steering)) steering = 0.0f;
            if (!float.IsFinite(steeringCenter)) steeringCenter = 0.0f;
            if (!float.IsFinite(pitch)) pitch = 0.0f;
            if (!float.IsFinite(roll)) roll = 0.0f;

            if (IsKeyPressed(KeyboardKey.R) && !remote.IsConnected)
                remote.TryConnect();
            if (IsKeyPressed(KeyboardKey.M))
            {
                mode = mode == DemoMode.Steering ? DemoMode.FlatOrientation : DemoMode.Steering;
                steeringCentered = false;
                pitch = 0.0f;
                roll = 0.0f;
            }
            if (mode == DemoMode.Steering && IsKeyPressed(KeyboardKey.C))
                steeringCentered = false;

            if (remote.TryGetAcceleration(out Vector3 acceleration) && IsFinite(acceleration))
            {
                float blend = 1.0f - MathF.Exp(-10.0f * GetFrameTime());
                if (mode == DemoMode.Steering)
                {
                    // With the buttons facing the user, gravity lies in the remote's
                    // X/Y plane. Its direction in that plane is the steering angle.
                    float gravityAngle = MathF.Atan2(acceleration.Y, -acceleration.X) * RadToDeg;
                    if (!steeringCentered)
                    {
                        steeringCenter = gravityAngle;
                        steering = 0.0f;
                        steeringCentered = true;
                    }

                    float targetSteering = WrapAngle(gravityAngle - steeringCenter);
                    steering = LerpAngle(steering, targetSteering, blend);
                }
                else
                {
                    float targetRoll = MathF.Atan2(-acceleration.X, acceleration.Z) * RadToDeg;
                    float targetPitch = MathF.Atan2(
                        -acceleration.Y,
                        MathF.Sqrt(acceleration.X * acceleration.X + acceleration.Z * acceleration.Z)) * RadToDeg;

                    roll = LerpAngle(roll, targetRoll, blend);
                    pitch = LerpAngle(pitch, targetPitch, blend);
                }
            }

            BeginDrawing();
            ClearBackground(new Color(18, 22, 30, 255));
            BeginMode3D(camera);
            DrawGrid(20, 0.5f);

            Rlgl.PushMatrix();
            if (mode == DemoMode.Steering)
            {
                Rlgl.Rotatef(steering, 0.0f, 0.0f, 1.0f);
                // Face the buttons toward the camera and hold the length horizontally.
                Rlgl.Rotatef(-90.0f, 0.0f, 1.0f, 0.0f);
            }
            else
            {
                Rlgl.Rotatef(roll, 0.0f, 0.0f, 1.0f);
                Rlgl.Rotatef(pitch, 1.0f, 0.0f, 0.0f);
                // Put the button face toward world-up for the flat-on-desk pose.
                Rlgl.Rotatef(90.0f, 0.0f, 0.0f, 1.0f);
            }
            Rlgl.Scalef(modelScale, modelScale, modelScale);
            Rlgl.Translatef(-modelCenter.X, -modelCenter.Y, -modelCenter.Z);
            DrawModel(model, Vector3.Zero, 1.0f, Color.White);
            Rlgl.PopMatrix();

            EndMode3D();
            string heading = mode == DemoMode.Steering
                ? "Wii Remote gravity steering"
                : "Wii Remote flat orientation";
            string angles = mode == DemoMode.Steering
                ? $"Steering {steering,7:0.0} deg"
                : $"Pitch {pitch,7:0.0} deg    Roll {roll,7:0.0} deg";
            DrawText(heading, 28, 24, 30, Color.RayWhite);
            DrawText(remote.Status, 30, 66, 20, remote.IsConnected ? Color.Lime : Color.Orange);
            DrawText(angles, 30, 98, 20, Color.LightGray);
            DrawText("M: switch mode     C: center steering     R: reconnect     Esc: quit", 30, ScreenHeight - 42, 18, Color.Gray);
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
