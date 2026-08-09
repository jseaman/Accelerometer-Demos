using System;
using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace WiiMoteVectorVisualizer;

internal static class Program
{
    private const int ScreenWidth = 1200;
    private const int ScreenHeight = 800;
    private const float MaximumAcceleration = 3.0f;
    private static readonly Color XColor = new(239, 74, 78, 255);
    private static readonly Color YColor = new(70, 135, 255, 255);
    private static readonly Color ZColor = new(77, 220, 120, 255);
    private static readonly Color ResultColor = new(255, 196, 66, 255);

    [STAThread]
    private static void Main()
    {
        SetConfigFlags(ConfigFlags.Msaa4xHint | ConfigFlags.VSyncHint);
        InitWindow(ScreenWidth, ScreenHeight, "Wii Remote acceleration vector visualizer");
        SetTargetFPS(60);

        float cameraAzimuth = 42.0f;
        float cameraElevation = 25.0f;
        float cameraDistance = 8.5f;
        Camera3D camera = CreateCamera(cameraAzimuth, cameraElevation, cameraDistance);

        using WiimoteConnection remote = new();
        remote.TryConnect();

        Vector3 sensorAcceleration = Vector3.Zero;
        Vector3 displayedWorldAcceleration = Vector3.Zero;

        while (!WindowShouldClose())
        {
            if (IsKeyPressed(KeyboardKey.R) && !remote.IsConnected)
                remote.TryConnect();

            if (IsMouseButtonDown(MouseButton.Left))
            {
                Vector2 mouseDelta = GetMouseDelta();
                cameraAzimuth -= mouseDelta.X * 0.25f;
                cameraElevation = Math.Clamp(cameraElevation + mouseDelta.Y * 0.25f, -80.0f, 80.0f);
            }

            cameraDistance = Math.Clamp(cameraDistance - GetMouseWheelMove() * 0.6f, 4.5f, 14.0f);
            camera = CreateCamera(cameraAzimuth, cameraElevation, cameraDistance);

            if (remote.TryGetAcceleration(out Vector3 acceleration))
            {
                sensorAcceleration = acceleration;
                // Wii Remote convention mapped into the scene:
                // sensor +X -> world +X, sensor +Y -> world -Z,
                // sensor +Z (out through the buttons) -> world +Y.
                Vector3 target = new(acceleration.X, acceleration.Z, -acceleration.Y);
                float blend = 1.0f - MathF.Exp(-14.0f * GetFrameTime());
                displayedWorldAcceleration = Vector3.Lerp(displayedWorldAcceleration, target, blend);
            }

            float measuredMagnitude = sensorAcceleration.Length();
            Vector3 resultVector = LimitMagnitude(displayedWorldAcceleration, MaximumAcceleration);

            BeginDrawing();
            ClearBackground(new Color(13, 18, 27, 255));
            BeginMode3D(camera);

            DrawGrid(20, 0.5f);
            DrawSphereWires(Vector3.Zero, MaximumAcceleration, 18, 24, new Color(125, 174, 220, 105));
            DrawSphereWires(Vector3.Zero, MaximumAcceleration, 9, 12, new Color(65, 100, 140, 80));

            DrawControllerBody();
            DrawReferenceAxes();

            Vector3 origin = new(0.0f, 0.20f, 0.0f);
            DrawArrow3D(origin, new Vector3(displayedWorldAcceleration.X, 0.0f, 0.0f), XColor, 0.035f);
            DrawArrow3D(origin, new Vector3(0.0f, 0.0f, displayedWorldAcceleration.Z), YColor, 0.035f);
            DrawArrow3D(origin, new Vector3(0.0f, displayedWorldAcceleration.Y, 0.0f), ZColor, 0.035f);
            DrawArrow3D(origin, resultVector, ResultColor, 0.065f);

            EndMode3D();

            DrawReferenceAxisLabels(camera);

            DrawText("Wii Remote acceleration vector", 28, 24, 30, Color.RayWhite);
            DrawText(remote.Status, 30, 66, 20, remote.IsConnected ? Color.Lime : Color.Orange);
            DrawText($"X lateral       {sensorAcceleration.X,8:0.000} g", 30, 112, 20, XColor);
            DrawText($"Y longitudinal  {sensorAcceleration.Y,8:0.000} g", 30, 140, 20, YColor);
            DrawText($"Z button-normal {sensorAcceleration.Z,8:0.000} g", 30, 168, 20, ZColor);
            DrawText($"Magnitude       {measuredMagnitude,8:0.000} g", 30, 204, 22,
                measuredMagnitude > MaximumAcceleration ? Color.Orange : ResultColor);
            DrawText("Wire sphere radius: 3 g", 30, 238, 18, new Color(125, 174, 220, 255));
            if (measuredMagnitude > MaximumAcceleration)
                DrawText("Display arrow clamped at 3 g", 30, 266, 18, Color.Orange);

            DrawText("Red: X     Blue: Y     Green: Z     Gold: resultant", 30, ScreenHeight - 70, 18, Color.LightGray);
            DrawText("Left-drag: orbit camera     Wheel: zoom     R: reconnect     Esc: quit", 30, ScreenHeight - 40, 18, Color.Gray);
            DrawFPS(ScreenWidth - 100, 20);
            EndDrawing();
        }

        CloseWindow();
    }

    private static Camera3D CreateCamera(float azimuthDegrees, float elevationDegrees, float distance)
    {
        float azimuth = azimuthDegrees * MathF.PI / 180.0f;
        float elevation = elevationDegrees * MathF.PI / 180.0f;
        float horizontal = distance * MathF.Cos(elevation);
        return new Camera3D
        {
            Position = new Vector3(
                horizontal * MathF.Sin(azimuth),
                distance * MathF.Sin(elevation),
                horizontal * MathF.Cos(azimuth)),
            Target = Vector3.Zero,
            Up = Vector3.UnitY,
            FovY = 45.0f,
            Projection = CameraProjection.Perspective
        };
    }

    private static void DrawControllerBody()
    {
        DrawCube(Vector3.Zero, 0.82f, 0.28f, 2.10f, new Color(238, 241, 246, 255));
        DrawCubeWires(Vector3.Zero, 0.82f, 0.28f, 2.10f, new Color(120, 128, 140, 255));
        DrawCube(new Vector3(0.0f, 0.15f, -0.58f), 0.28f, 0.035f, 0.22f, new Color(175, 180, 188, 255));
        DrawCube(new Vector3(0.0f, 0.15f, 0.08f), 0.20f, 0.035f, 0.20f, new Color(175, 180, 188, 255));
    }

    private static void DrawReferenceAxes()
    {
        Vector3 origin = Vector3.Zero;
        Vector3 xEnd = new(3.35f, 0.0f, 0.0f);
        Vector3 yEnd = new(0.0f, 0.0f, -3.35f);
        Vector3 zEnd = new(0.0f, 3.35f, 0.0f);
        DrawLine3D(origin, xEnd, Fade(XColor, 0.50f));
        DrawLine3D(origin, yEnd, Fade(YColor, 0.50f));
        DrawLine3D(origin, zEnd, Fade(ZColor, 0.50f));

    }

    private static void DrawReferenceAxisLabels(Camera3D camera)
    {
        DrawWorldLabel(camera, new Vector3(3.35f, 0.0f, 0.0f), "+X lateral", XColor);
        DrawWorldLabel(camera, new Vector3(0.0f, 0.0f, -3.35f), "+Y longitudinal", YColor);
        DrawWorldLabel(camera, new Vector3(0.0f, 3.35f, 0.0f), "+Z buttons", ZColor);
    }

    private static void DrawWorldLabel(Camera3D camera, Vector3 position, string text, Color color)
    {
        Vector2 screen = GetWorldToScreen(position, camera);
        DrawText(text, (int)screen.X + 5, (int)screen.Y - 10, 17, color);
    }

    private static void DrawArrow3D(Vector3 origin, Vector3 vector, Color color, float shaftRadius)
    {
        float length = vector.Length();
        if (length < 0.015f)
            return;

        Vector3 direction = vector / length;
        float headLength = MathF.Min(0.28f, length * 0.35f);
        Vector3 tip = origin + vector;
        Vector3 headBase = tip - direction * headLength;
        if (Vector3.DistanceSquared(origin, headBase) > 0.0001f)
            DrawCylinderEx(origin, headBase, shaftRadius, shaftRadius, 12, color);
        DrawCylinderEx(headBase, tip, shaftRadius * 3.2f, 0.0f, 16, color);
    }

    private static Vector3 LimitMagnitude(Vector3 vector, float maximum)
    {
        float length = vector.Length();
        return length > maximum && length > 0.0f ? vector * (maximum / length) : vector;
    }
}
