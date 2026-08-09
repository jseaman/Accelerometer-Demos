using System;
using System.Numerics;
using Raylib_cs;
using WiimoteLib;
using static Raylib_cs.Raylib;

namespace WiiMoteIRDemo;

internal static class Program
{
    private const int CameraWidth = 1024;
    private const int CameraHeight = 768;
    private const int ScreenWidth = 1100;
    private const int ScreenHeight = 900;
    private const float PointerLeft = -340.0f;
    private const float PointerRight = 340.0f;
    private const float BottomBarTop = -290.0f;
    private const float BottomBarBottom = 92.0f;
    private static readonly Rectangle CameraView = new(38, 78, CameraWidth, CameraHeight);

    [STAThread]
    private static void Main()
    {
        SetConfigFlags(ConfigFlags.Msaa4xHint | ConfigFlags.VSyncHint);
        InitWindow(ScreenWidth, ScreenHeight, "WiiMote IR camera viewer");
        SetTargetFPS(60);

        using WiimoteIrConnection remote = new();
        remote.TryConnect();

        bool showCursor = false;
        bool havePointer = false;
        Vector2 pointer = Vector2.Zero;
        float baselineAngle = 0.0f;
        IRState irState = default;

        while (!WindowShouldClose())
        {
            if (IsKeyPressed(KeyboardKey.R) && !remote.IsConnected)
                remote.TryConnect();
            if (IsKeyPressed(KeyboardKey.C))
                showCursor = !showCursor;

            if (remote.TryGetIrState(out IRState latest))
            {
                irState = latest;
                IRSensor first = latest.IRSensors[0];
                IRSensor second = latest.IRSensors[1];
                if (first.Found && second.Found)
                {
                    Vector2 p1 = CameraPoint(first.RawPosition);
                    Vector2 p2 = CameraPoint(second.RawPosition);
                    Vector2 targetPointer = CalculateBottomBarCursor(first.RawPosition, second.RawPosition);
                    float targetAngle = NormalizeBaselineAngle(
                        MathF.Atan2(p2.Y - p1.Y, p2.X - p1.X) * 180.0f / MathF.PI);
                    float blend = 1.0f - MathF.Exp(-18.0f * GetFrameTime());

                    if (!havePointer)
                    {
                        pointer = targetPointer;
                        baselineAngle = targetAngle;
                        havePointer = true;
                    }
                    else
                    {
                        pointer = Vector2.Lerp(pointer, targetPointer, blend);
                        baselineAngle = LerpAngle(baselineAngle, targetAngle, blend);
                    }
                }
                else
                {
                    havePointer = false;
                }
            }

            BeginDrawing();
            ClearBackground(new Color(14, 18, 25, 255));
            DrawText("Wii Remote IR camera - 1024 x 768", 38, 24, 28, Color.RayWhite);
            DrawText(remote.Status, 570, 31, 18, remote.IsConnected ? Color.Lime : Color.Orange);
            DrawCameraField();

            if (irState.IRSensors is { Length: >= 2 })
            {
                DrawBlob(irState.IRSensors[0], 1, Color.Red);
                DrawBlob(irState.IRSensors[1], 2, Color.SkyBlue);

                if (irState.IRSensors[0].Found && irState.IRSensors[1].Found)
                {
                    Vector2 first = CameraPoint(irState.IRSensors[0].RawPosition);
                    Vector2 second = CameraPoint(irState.IRSensors[1].RawPosition);
                    Vector2 rawMidpoint = (first + second) * 0.5f;
                    DrawLineEx(first, second, 2.0f, Color.Gold);
                    DrawCircleV(rawMidpoint, 5.0f, Color.Gold);
                    DrawLine((int)rawMidpoint.X - 10, (int)rawMidpoint.Y, (int)rawMidpoint.X + 10, (int)rawMidpoint.Y, Color.Gold);
                    DrawLine((int)rawMidpoint.X, (int)rawMidpoint.Y - 10, (int)rawMidpoint.X, (int)rawMidpoint.Y + 10, Color.Gold);

                    string angleText = $"Blob baseline: {baselineAngle,7:0.0} deg";
                    DrawText(angleText, 48, 800, 18, Color.Gold);
                    DrawText($"Bottom-bar cursor: ({pointer.X - CameraView.X,7:0.0}, {pointer.Y - CameraView.Y,7:0.0})", 330, 800, 18, Color.Gold);

                    if (showCursor)
                        DrawRotatedCursor(pointer, -baselineAngle);
                }
            }

            string cursorState = showCursor ? "ON" : "OFF";
            string trackingState = havePointer ? "two blobs tracked" : "waiting for two IR blobs";
            DrawText($"Bar: BOTTOM     C: cursor {cursorState}     R: reconnect     Esc: quit", 38, 862, 18, Color.LightGray);
            DrawText(trackingState, 720, 862, 18, havePointer ? Color.Lime : Color.Orange);
            EndDrawing();
        }

        CloseWindow();
    }

    private static void DrawCameraField()
    {
        DrawRectangleRec(CameraView, new Color(5, 8, 13, 255));
        for (int x = 0; x <= CameraWidth; x += 128)
            DrawLine((int)CameraView.X + x, (int)CameraView.Y, (int)CameraView.X + x, (int)(CameraView.Y + CameraView.Height), new Color(27, 38, 52, 255));
        for (int y = 0; y <= CameraHeight; y += 96)
            DrawLine((int)CameraView.X, (int)CameraView.Y + y, (int)(CameraView.X + CameraView.Width), (int)CameraView.Y + y, new Color(27, 38, 52, 255));
        DrawRectangleLinesEx(CameraView, 2.0f, Color.Gray);
        DrawText("(0,767)", 44, 84, 15, Color.Gray);
        DrawText("(1023,0)", 975, 824, 15, Color.Gray);
    }

    private static void DrawBlob(IRSensor sensor, int number, Color color)
    {
        if (!sensor.Found)
        {
            DrawText($"IR {number}: not visible", 48 + (number - 1) * 190, 824, 16, color);
            return;
        }

        Vector2 point = CameraPoint(sensor.RawPosition);
        float radius = 10.0f + sensor.Size * 1.5f;
        DrawCircleV(point, radius + 6.0f, Fade(color, 0.20f));
        DrawCircleLines((int)point.X, (int)point.Y, radius, color);
        DrawCircleV(point, 4.0f, color);

        string label = $"IR {number}  ({sensor.RawPosition.X}, {sensor.RawPosition.Y})";
        int labelX = Math.Clamp((int)point.X + 14, (int)CameraView.X + 4, (int)(CameraView.X + CameraView.Width) - 190);
        int labelY = Math.Clamp((int)point.Y - 24, (int)CameraView.Y + 4, (int)(CameraView.Y + CameraView.Height) - 24);
        DrawText(label, labelX, labelY, 18, color);
    }

    private static Vector2 CameraPoint(Point raw)
    {
        float x = CameraView.X + Math.Clamp(raw.X, 0, CameraWidth - 1);
        // Native IR coordinates use bottom-left as their origin.
        float y = CameraView.Y + (CameraHeight - 1 - Math.Clamp(raw.Y, 0, CameraHeight - 1));
        return new Vector2(x, y);
    }

    private static Vector2 CalculateBottomBarCursor(Point first, Point second)
    {
        // Convert native bottom-left camera coordinates to the bar-relative
        // coordinate frame used by the established Wiimote pointer algorithm.
        Vector2 a = new(-(first.X - 512.0f), first.Y - 384.0f);
        Vector2 b = new(-(second.X - 512.0f), second.Y - 384.0f);
        Vector2 difference = b - a;
        float correction = -MathF.Atan2(difference.Y, difference.X);
        Vector2 midpoint = (a + b) * 0.5f;
        float cosine = MathF.Cos(correction);
        float sine = MathF.Sin(correction);
        Vector2 aligned = new(
            midpoint.X * cosine - midpoint.Y * sine,
            midpoint.X * sine + midpoint.Y * cosine);

        float normalizedX = (aligned.X - PointerLeft) / (PointerRight - PointerLeft);
        float normalizedY = (aligned.Y - BottomBarTop) / (BottomBarBottom - BottomBarTop);
        return new Vector2(
            CameraView.X + Math.Clamp(normalizedX, 0.0f, 1.0f) * CameraView.Width,
            CameraView.Y + Math.Clamp(normalizedY, 0.0f, 1.0f) * CameraView.Height);
    }

    private static void DrawRotatedCursor(Vector2 center, float baselineDegrees)
    {
        // The cursor points up when the two sensor-bar lights are horizontal,
        // then rolls with the line joining the blobs.
        Vector2 tip = Rotate(new Vector2(0, -30), baselineDegrees) + center;
        Vector2 left = Rotate(new Vector2(-13, 16), baselineDegrees) + center;
        Vector2 right = Rotate(new Vector2(13, 16), baselineDegrees) + center;
        DrawTriangle(tip, left, right, Color.Lime);
        DrawTriangleLines(tip, left, right, Color.Black);
    }

    private static Vector2 Rotate(Vector2 vector, float degrees)
    {
        float radians = degrees * MathF.PI / 180.0f;
        float cosine = MathF.Cos(radians);
        float sine = MathF.Sin(radians);
        return new Vector2(vector.X * cosine - vector.Y * sine, vector.X * sine + vector.Y * cosine);
    }

    private static float LerpAngle(float current, float target, float amount)
    {
        float delta = (target - current + 540.0f) % 360.0f - 180.0f;
        return current + delta * amount;
    }

    private static float NormalizeBaselineAngle(float angle)
    {
        // A sensor-bar baseline has no inherent start/end direction. Collapse
        // equivalent 180-degree results so a level bar is always zero degrees.
        while (angle > 90.0f) angle -= 180.0f;
        while (angle < -90.0f) angle += 180.0f;
        return angle;
    }
}
