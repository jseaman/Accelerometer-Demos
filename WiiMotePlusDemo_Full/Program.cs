using System;
using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace WiiMotePlusDemo_Full;

internal static class Program
{
    private const int ScreenWidth = 1100;
    private const int ScreenHeight = 720;
    private const float RadToDeg = 180.0f / MathF.PI;
    private const float DegToRad = MathF.PI / 180.0f;
    private const float GravityCorrectionRate = 2.0f;

    [STAThread]
    private static void Main()
    {
        SetConfigFlags(ConfigFlags.Msaa4xHint | ConfigFlags.VSyncHint);
        InitWindow(ScreenWidth, ScreenHeight, "Wii MotionPlus - full quaternion fusion");
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
        Vector3 measuredGravity = Vector3.UnitY;
        Vector3 gyroRates = Vector3.Zero;
        bool orientationInitialized = false;

        while (!WindowShouldClose())
        {
            if (IsKeyPressed(KeyboardKey.R) && !remote.IsConnected)
                remote.TryConnect();
            if (IsKeyPressed(KeyboardKey.C) && remote.IsConnected)
            {
                orientation = ShortestArc(Vector3.UnitY, measuredGravity);
                remote.StartCalibration();
            }

            float dt = MathF.Min(GetFrameTime(), 0.05f);
            if (remote.TryGetMotion(out MotionSample motion))
            {
                bool gyroValid = IsFinite(motion.GyroDegreesPerSecond);
                if (gyroValid)
                    gyroRates = motion.GyroDegreesPerSecond;

                Vector3 acceleration = motion.Acceleration;
                Vector3 gravitySample = new(acceleration.X, acceleration.Z, -acceleration.Y);
                float accelerationMagnitude = gravitySample.Length();
                bool gravityValid = IsFinite(acceleration) &&
                    float.IsFinite(accelerationMagnitude) && accelerationMagnitude > 0.0001f;

                if (gravityValid)
                {
                    measuredGravity = gravitySample / accelerationMagnitude;
                    if (!orientationInitialized)
                    {
                        orientation = ShortestArc(Vector3.UnitY, measuredGravity);
                        orientationInitialized = true;
                    }
                }

                if (!IsFinite(orientation))
                {
                    orientation = Quaternion.Identity;
                    orientationInitialized = false;
                }

                if (remote.IsCalibrated && gyroValid)
                {
                    // WiimoteLib fields are X=yaw, Y=roll, Z=pitch. Convert
                    // those device-local rates into the corrected model basis.
                    // The signs retain WiiMotePlusDemo's verified yaw direction.
                    Vector3 bodyRate = new(-gyroRates.Z, -gyroRates.X, gyroRates.Y);
                    orientation = IntegrateBodyRates(orientation, bodyRate, dt);
                }

                // Gravity only corrects the two observable tilt degrees of
                // freedom. The correction is perpendicular to gravity, so it
                // does not erase gyro twist/yaw when the remote is upright.
                if (gravityValid)
                {
                    float gravityConfidence = Math.Clamp(
                        1.0f - MathF.Abs(accelerationMagnitude - 1.0f) / 0.35f,
                        0.0f, 1.0f);
                    if (gravityConfidence > 0.0f)
                    {
                        Vector3 predictedGravity = Vector3.Transform(Vector3.UnitY, orientation);
                        Quaternion fullCorrection = ShortestArc(predictedGravity, measuredGravity);
                        float correctionBlend = (1.0f - MathF.Exp(-GravityCorrectionRate * dt)) * gravityConfidence;
                        Quaternion correction = Quaternion.Slerp(Quaternion.Identity, fullCorrection, correctionBlend);
                        orientation = Quaternion.Normalize(correction * orientation);
                    }
                }
            }

            SetWindowTitle($"Wii MotionPlus - full quaternion fusion | {remote.Status}");

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
            GetDisplayAngles(measuredGravity, orientation, out float pitch, out float roll, out float heading);
            DrawText("Wii Remote Plus - full 3-axis quaternion fusion", 28, 24, 28, Color.RayWhite);
            DrawText(remote.Status, 30, 66, 20, remote.IsConnected ? Color.Lime : Color.Orange);
            DrawText($"Pitch {pitch,7:0.0} deg    Roll {roll,7:0.0} deg    Heading {heading,7:0.0} deg", 30, 98, 20, Color.LightGray);
            DrawText($"Gyro deg/s: yaw {gyroRates.X,7:0.0}   roll {gyroRates.Y,7:0.0}   pitch {gyroRates.Z,7:0.0}", 30, 128, 18, Color.SkyBlue);
            DrawText("C: center + calibrate gyro     R: reconnect     Esc: quit", 30, ScreenHeight - 42, 18, Color.Gray);
            DrawFPS(ScreenWidth - 100, 20);
            EndDrawing();
        }

        UnloadModel(model);
        CloseWindow();
    }

    private static Quaternion IntegrateBodyRates(Quaternion orientation, Vector3 degreesPerSecond, float dt)
    {
        if (!IsFinite(orientation) || !IsFinite(degreesPerSecond) || !float.IsFinite(dt))
            return Quaternion.Identity;

        float speed = degreesPerSecond.Length();
        if (!float.IsFinite(speed) || speed < 0.0001f)
            return orientation;

        Quaternion increment = Quaternion.CreateFromAxisAngle(
            degreesPerSecond / speed,
            speed * dt * DegToRad);
        return Quaternion.Normalize(orientation * increment);
    }

    private static Quaternion ShortestArc(Vector3 from, Vector3 to)
    {
        from = Vector3.Normalize(from);
        to = Vector3.Normalize(to);
        float dot = Math.Clamp(Vector3.Dot(from, to), -1.0f, 1.0f);
        if (dot > 0.999999f)
            return Quaternion.Identity;
        if (dot < -0.999999f)
        {
            Vector3 fallback = MathF.Abs(from.X) < 0.8f ? Vector3.UnitX : Vector3.UnitZ;
            Vector3 axis = Vector3.Normalize(Vector3.Cross(from, fallback));
            return Quaternion.CreateFromAxisAngle(axis, MathF.PI);
        }

        Vector3 rotationAxis = Vector3.Normalize(Vector3.Cross(from, to));
        return Quaternion.CreateFromAxisAngle(rotationAxis, MathF.Acos(dot));
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

    private static void GetDisplayAngles(
        Vector3 gravity, Quaternion orientation,
        out float pitch, out float roll, out float heading)
    {
        // Display-only Euler-like values; rendering never uses these angles.
        pitch = MathF.Atan2(-gravity.Z, MathF.Sqrt(gravity.X * gravity.X + gravity.Y * gravity.Y)) * RadToDeg;
        roll = MathF.Atan2(gravity.X, gravity.Y) * RadToDeg;
        Vector3 forward = Vector3.Transform(Vector3.UnitZ, orientation);
        heading = MathF.Atan2(forward.X, forward.Z) * RadToDeg;
    }
}
