using System;
using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace WiiMotePlusDemo_Original;

internal static class Program
{
    private const int ScreenWidth = 1100;
    private const int ScreenHeight = 720;
    private const float RadToDeg = 180.0f / MathF.PI;

    [STAThread]
    private static void Main()
    {
        SetConfigFlags(ConfigFlags.Msaa4xHint | ConfigFlags.VSyncHint);
        InitWindow(ScreenWidth, ScreenHeight, "Wii MotionPlus - original gravity orientation");
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

        Quaternion gravityOrientation = Quaternion.Identity;
        Vector3 gravity = Vector3.UnitY;
        float yaw = 0.0f;

        while (!WindowShouldClose())
        {
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
                // This is the rebuilt original WiiMoteDemo gravity algorithm:
                // map sensor gravity to world space, then use its minimum-twist
                // shortest-arc quaternion instead of unstable Euler rotations.
                Vector3 acceleration = motion.Acceleration;
                Vector3 measuredGravity = new(acceleration.X, acceleration.Z, -acceleration.Y);
                float magnitudeSquared = measuredGravity.LengthSquared();
                if (IsFinite(acceleration) && float.IsFinite(magnitudeSquared) && magnitudeSquared > 0.0001f)
                {
                    gravity = measuredGravity / MathF.Sqrt(magnitudeSquared);
                    Quaternion target = ShortestArc(Vector3.UnitY, gravity);
                    float blend = 1.0f - MathF.Exp(-10.0f * dt);
                    if (!IsFinite(gravityOrientation))
                        gravityOrientation = Quaternion.Identity;
                    gravityOrientation = Quaternion.Normalize(
                        Quaternion.Slerp(gravityOrientation, target, blend));
                }

                // Kept exactly as in WiiMotePlusDemo: calibrated MotionPlus yaw
                // rate is integrated with the verified physical direction.
                if (remote.IsCalibrated && IsFinite(motion.GyroDegreesPerSecond))
                    yaw = WrapAngle(yaw - motion.GyroDegreesPerSecond.X * dt);
            }

            SetWindowTitle($"Wii MotionPlus - original gravity orientation | {remote.Status}");

            BeginDrawing();
            ClearBackground(new Color(18, 22, 30, 255));
            BeginMode3D(camera);
            DrawGrid(20, 0.5f);

            Rlgl.PushMatrix();
            Rlgl.Rotatef(yaw, 0.0f, 1.0f, 0.0f);
            ApplyQuaternion(gravityOrientation);
            // GLB local +X is its button face; rotate it toward world-up.
            Rlgl.Rotatef(90.0f, 0.0f, 0.0f, 1.0f);
            Rlgl.Scalef(modelScale, modelScale, modelScale);
            Rlgl.Translatef(-modelCenter.X, -modelCenter.Y, -modelCenter.Z);
            DrawModel(model, Vector3.Zero, 1.0f, Color.White);
            Rlgl.PopMatrix();

            EndMode3D();
            GetDisplayAngles(gravity, out float pitch, out float roll, out float tilt);
            DrawText("Wii Remote Plus - stable gravity + MotionPlus yaw", 28, 24, 28, Color.RayWhite);
            DrawText(remote.Status, 30, 66, 20, remote.IsConnected ? Color.Lime : Color.Orange);
            DrawText($"Pitch {pitch,7:0.0} deg    Roll {roll,7:0.0} deg    Tilt {tilt,7:0.0} deg    Yaw {yaw,7:0.0} deg", 30, 98, 20, Color.LightGray);
            DrawText("C: center + calibrate gyro     R: reconnect     Esc: quit", 30, ScreenHeight - 42, 18, Color.Gray);
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
        pitch = MathF.Atan2(-gravity.Z, MathF.Sqrt(gravity.X * gravity.X + gravity.Y * gravity.Y)) * RadToDeg;
        roll = MathF.Atan2(gravity.X, gravity.Y) * RadToDeg;
        tilt = MathF.Acos(Math.Clamp(gravity.Y, -1.0f, 1.0f)) * RadToDeg;
    }

    private static float WrapAngle(float angle)
    {
        while (angle > 180.0f) angle -= 360.0f;
        while (angle < -180.0f) angle += 360.0f;
        return angle;
    }
}
