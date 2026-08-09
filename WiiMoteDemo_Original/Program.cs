using System;
using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace WiiMoteDemo_Original;

internal static class Program
{
    private const int ScreenWidth = 1100;
    private const int ScreenHeight = 720;
    private const float RadToDeg = 180.0f / MathF.PI;

    [STAThread]
    private static void Main()
    {
        SetConfigFlags(ConfigFlags.Msaa4xHint | ConfigFlags.VSyncHint);
        InitWindow(ScreenWidth, ScreenHeight, "Wii Remote stable gravity orientation");
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

        Quaternion orientation = Quaternion.Identity;
        Vector3 gravity = Vector3.UnitY;

        while (!WindowShouldClose())
        {
            if (IsKeyPressed(KeyboardKey.R) && !remote.IsConnected)
                remote.TryConnect();

            if (remote.TryGetAcceleration(out Vector3 acceleration) && IsFinite(acceleration))
            {
                // Map Wiimote sensor coordinates into the corrected raylib world:
                // flat/buttons-up (0,0,+1) becomes world-up (0,+1,0).
                Vector3 measuredGravity = new(acceleration.X, acceleration.Z, -acceleration.Y);
                float magnitudeSquared = measuredGravity.LengthSquared();
                if (float.IsFinite(magnitudeSquared) && magnitudeSquared > 0.0001f)
                {
                    gravity = measuredGravity / MathF.Sqrt(magnitudeSquared);
                    Quaternion target = ShortestArc(Vector3.UnitY, gravity);
                    float blend = 1.0f - MathF.Exp(-10.0f * MathF.Min(GetFrameTime(), 0.05f));
                    if (!IsFinite(orientation))
                        orientation = Quaternion.Identity;
                    orientation = Quaternion.Normalize(Quaternion.Slerp(orientation, target, blend));
                }
            }

            BeginDrawing();
            ClearBackground(new Color(18, 22, 30, 255));
            BeginMode3D(camera);
            DrawGrid(20, 0.5f);

            Rlgl.PushMatrix();
            ApplyQuaternion(orientation);
            // GLB local +X is its button face; rotate it toward world-up.
            Rlgl.Rotatef(90.0f, 0.0f, 0.0f, 1.0f);
            Rlgl.Scalef(modelScale, modelScale, modelScale);
            Rlgl.Translatef(-modelCenter.X, -modelCenter.Y, -modelCenter.Z);
            DrawModel(model, Vector3.Zero, 1.0f, Color.White);
            Rlgl.PopMatrix();

            EndMode3D();

            GetDisplayAngles(gravity, out float pitch, out float roll, out float tilt);
            DrawText("Stable gravity orientation (original algorithm rebuilt)", 28, 24, 28, Color.RayWhite);
            DrawText(remote.Status, 30, 66, 20, remote.IsConnected ? Color.Lime : Color.Orange);
            DrawText($"Pitch {pitch,7:0.0} deg    Roll {roll,7:0.0} deg    Tilt {tilt,7:0.0} deg", 30, 98, 20, Color.LightGray);
            DrawText("Yaw/twist around gravity is not observable without gyro or IR", 30, 128, 18, Color.Orange);
            DrawText("R: reconnect     Esc: quit", 30, ScreenHeight - 42, 18, Color.Gray);
            DrawFPS(ScreenWidth - 100, 20);
            EndDrawing();
        }

        UnloadModel(model);
        CloseWindow();
    }

    private static Quaternion ShortestArc(Vector3 from, Vector3 to)
    {
        float dot = Math.Clamp(Vector3.Dot(from, to), -1.0f, 1.0f);
        if (dot > 0.999999f)
            return Quaternion.Identity;

        if (dot < -0.999999f)
            return Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathF.PI);

        Vector3 axis = Vector3.Normalize(Vector3.Cross(from, to));
        return Quaternion.CreateFromAxisAngle(axis, MathF.Acos(dot));
    }

    private static void ApplyQuaternion(Quaternion quaternion)
    {
        if (!IsFinite(quaternion))
            return;

        quaternion = Quaternion.Normalize(quaternion);
        float halfAngleSin = MathF.Sqrt(MathF.Max(0.0f, 1.0f - quaternion.W * quaternion.W));
        if (halfAngleSin < 0.00001f)
            return;

        float angle = 2.0f * MathF.Acos(Math.Clamp(quaternion.W, -1.0f, 1.0f)) * RadToDeg;
        Rlgl.Rotatef(angle,
            quaternion.X / halfAngleSin,
            quaternion.Y / halfAngleSin,
            quaternion.Z / halfAngleSin);
    }

    private static bool IsFinite(Vector3 value) =>
        float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Z);

    private static bool IsFinite(Quaternion value) =>
        float.IsFinite(value.X) && float.IsFinite(value.Y) &&
        float.IsFinite(value.Z) && float.IsFinite(value.W);

    private static void GetDisplayAngles(Vector3 gravity, out float pitch, out float roll, out float tilt)
    {
        // These labels describe the gravity vector only; they do not drive rendering.
        pitch = MathF.Atan2(-gravity.Z, MathF.Sqrt(gravity.X * gravity.X + gravity.Y * gravity.Y)) * RadToDeg;
        roll = MathF.Atan2(gravity.X, gravity.Y) * RadToDeg;
        tilt = MathF.Acos(Math.Clamp(gravity.Y, -1.0f, 1.0f)) * RadToDeg;
    }
}
