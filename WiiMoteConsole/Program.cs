using System;
using System.IO;
using System.Numerics;
using System.Text;
using System.Threading;
using WiimoteLib;

namespace WiiMoteConsole;

internal static class Program
{
    private const int CalibrationSamples = 120;
    private static bool running = true;
    private static Vector3 gyroBias;
    private static Vector3 gyroSum;
    private static int gyroSampleCount;

    private static int Main()
    {
        Console.Title = "WiiMoteConsole - raw Wii Remote monitor";
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            running = false;
        };

        using Wiimote wiimote = new();
        try
        {
            Console.WriteLine("Connecting to Wii Remote...");
            wiimote.Connect();
            bool hasMotionPlus = TryInitializeMotionPlus(wiimote);
            bool hasExtension = wiimote.WiimoteState.ExtensionType != ExtensionType.None;
            wiimote.SetReportType(
                hasExtension ? InputReport.IRExtensionAccel : InputReport.IRAccel,
                IRSensitivity.WiiLevel3,
                true);
            wiimote.SetLEDs(1);
            ResetGyroCalibration();

            try { Console.CursorVisible = false; } catch { }

            while (running)
            {
                HandleKeyboard();
                WiimoteState state = wiimote.WiimoteState;
                if (hasMotionPlus)
                    UpdateGyroCalibration(state);
                DrawDashboard(state);
                Thread.Sleep(33);
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("Wii Remote error: " + ex.Message);
            Console.WriteLine("Close Dolphin, wake the remote, and run this program again.");
            return 1;
        }
        finally
        {
            try
            {
                wiimote.SetLEDs(0);
                wiimote.Disconnect();
            }
            catch
            {
                // The controller may already be disconnected.
            }

            try { Console.CursorVisible = true; } catch { }
        }
    }

    private static bool TryInitializeMotionPlus(Wiimote wiimote)
    {
        try
        {
            wiimote.InitializeMotionPlus();
            Thread.Sleep(750);
            return wiimote.WiimoteState.ExtensionType == ExtensionType.MotionPlus;
        }
        catch
        {
            // Original RVL-CNT-01 remotes have no built-in gyroscope. They
            // remain fully usable for accelerometer, IR, buttons, and battery.
            return false;
        }
    }

    private static void HandleKeyboard()
    {
        try
        {
            while (Console.KeyAvailable)
            {
                ConsoleKey key = Console.ReadKey(true).Key;
                if (key is ConsoleKey.Q or ConsoleKey.Escape)
                    running = false;
                else if (key == ConsoleKey.C)
                    ResetGyroCalibration();
            }
        }
        catch (InvalidOperationException)
        {
            // Input is redirected; Ctrl+C can still stop the monitor.
        }
    }

    private static void ResetGyroCalibration()
    {
        gyroBias = Vector3.Zero;
        gyroSum = Vector3.Zero;
        gyroSampleCount = 0;
    }

    private static void UpdateGyroCalibration(WiimoteState state)
    {
        if (state.ExtensionType != ExtensionType.MotionPlus || gyroSampleCount >= CalibrationSamples)
            return;

        Point3 raw = state.MotionPlusState.RawValues;
        gyroSum += new Vector3(raw.X, raw.Y, raw.Z);
        gyroSampleCount++;
        if (gyroSampleCount == CalibrationSamples)
            gyroBias = gyroSum / CalibrationSamples;
    }

    private static void DrawDashboard(WiimoteState state)
    {
        AccelState accel = state.AccelState;
        bool hasMotionPlus = state.ExtensionType == ExtensionType.MotionPlus;

        StringBuilder text = new();
        text.AppendLine("WiiMoteConsole - live raw device values");
        text.AppendLine("============================================================");
        text.AppendLine($"Extension       : {state.ExtensionType}");
        text.AppendLine($"Battery         : raw={state.BatteryRaw,3}  normalized={state.Battery,7:0.000}");
        text.AppendLine();
        text.AppendLine("ACCELEROMETER");
        text.AppendLine($"  Raw X/Y/Z     : {accel.RawValues.X,6}  {accel.RawValues.Y,6}  {accel.RawValues.Z,6}");
        text.AppendLine($"  G   X/Y/Z     : {accel.Values.X,9:0.0000}  {accel.Values.Y,9:0.0000}  {accel.Values.Z,9:0.0000}");
        text.AppendLine();
        if (state.ExtensionType == ExtensionType.Nunchuk)
        {
            NunchukState nunchuk = state.NunchukState;
            text.AppendLine("NUNCHUK");
            text.AppendLine($"  Buttons C/Z       : C={nunchuk.C,-5}  Z={nunchuk.Z,-5}");
            text.AppendLine($"  Joystick raw X/Y  : {nunchuk.RawJoystick.X,6}  {nunchuk.RawJoystick.Y,6}");
            text.AppendLine($"  Joystick norm X/Y : {nunchuk.Joystick.X,9:0.0000}  {nunchuk.Joystick.Y,9:0.0000}");
            text.AppendLine($"  Accel raw X/Y/Z   : {nunchuk.AccelState.RawValues.X,6}  {nunchuk.AccelState.RawValues.Y,6}  {nunchuk.AccelState.RawValues.Z,6}");
            text.AppendLine($"  Accel G   X/Y/Z   : {nunchuk.AccelState.Values.X,9:0.0000}  {nunchuk.AccelState.Values.Y,9:0.0000}  {nunchuk.AccelState.Values.Z,9:0.0000}");
            text.AppendLine();
        }
        if (hasMotionPlus)
        {
            MotionPlusState motion = state.MotionPlusState;
            Vector3 rawGyro = new(motion.RawValues.X, motion.RawValues.Y, motion.RawValues.Z);
            bool calibrated = gyroSampleCount >= CalibrationSamples;
            Vector3 velocity = calibrated ? ConvertGyroVelocity(rawGyro - gyroBias, motion) : Vector3.Zero;

            text.AppendLine("MOTIONPLUS GYROSCOPE");
            text.AppendLine($"  Raw yaw/roll/pitch : {motion.RawValues.X,6}  {motion.RawValues.Y,6}  {motion.RawValues.Z,6}");
            text.AppendLine($"  Fast flags         : yaw={motion.YawFast,-5} roll={motion.RollFast,-5} pitch={motion.PitchFast,-5}");
            text.AppendLine($"  Zero bias          : {gyroBias.X,9:0.0}  {gyroBias.Y,9:0.0}  {gyroBias.Z,9:0.0}");
            text.AppendLine($"  Velocity deg/s     : yaw={velocity.X,9:0.000}  roll={velocity.Y,9:0.000}  pitch={velocity.Z,9:0.000}");
            text.AppendLine(calibrated
                ? "  Calibration        : READY"
                : $"  Calibration        : KEEP STILL ({gyroSampleCount}/{CalibrationSamples})");
            text.AppendLine();
        }
        text.AppendLine($"IR CAMERA - mode: {state.IRState.Mode}");

        IRSensor[]? sensors = state.IRState.IRSensors;
        for (int i = 0; i < 4; i++)
        {
            if (sensors is not null && i < sensors.Length)
            {
                IRSensor sensor = sensors[i];
                text.AppendLine($"  IR {i + 1}: found={sensor.Found,-5} raw=({sensor.RawPosition.X,4},{sensor.RawPosition.Y,4}) " +
                    $"normalized=({sensor.Position.X,7:0.0000},{sensor.Position.Y,7:0.0000}) size={sensor.Size,2}");
            }
            else
            {
                text.AppendLine($"  IR {i + 1}: unavailable in current report");
            }
        }

        text.AppendLine($"  Midpoint raw       : ({state.IRState.RawMidpoint.X,4},{state.IRState.RawMidpoint.Y,4})");
        text.AppendLine($"  Midpoint normalized: ({state.IRState.Midpoint.X,7:0.0000},{state.IRState.Midpoint.Y,7:0.0000})");
        text.AppendLine();
        ButtonState buttons = state.ButtonState;
        text.AppendLine("BUTTONS");
        text.AppendLine($"  A={buttons.A} B={buttons.B} 1={buttons.One} 2={buttons.Two} +={buttons.Plus} -={buttons.Minus} Home={buttons.Home}");
        text.AppendLine($"  Up={buttons.Up} Down={buttons.Down} Left={buttons.Left} Right={buttons.Right}");
        text.AppendLine();
        text.AppendLine(hasMotionPlus
            ? "C: recalibrate gyro    Q/Esc: quit    Ctrl+C: quit"
            : "No MotionPlus gyro detected    Q/Esc: quit    Ctrl+C: quit");

        try
        {
            Console.SetCursorPosition(0, 0);
            int width = Math.Max(1, Console.BufferWidth - 1);
            foreach (string line in text.ToString().Replace("\r", string.Empty).Split('\n'))
                Console.WriteLine(line.Length >= width ? line[..width] : line.PadRight(width));
        }
        catch (IOException)
        {
            Console.Write(text.ToString());
        }
        catch (ArgumentOutOfRangeException)
        {
            Console.Write(text.ToString());
        }
    }

    private static Vector3 ConvertGyroVelocity(Vector3 delta, MotionPlusState motion)
    {
        return new Vector3(
            delta.X / (motion.YawFast ? 4.0f : 20.0f),
            delta.Y / (motion.RollFast ? 4.0f : 20.0f),
            delta.Z / (motion.PitchFast ? 4.0f : 20.0f));
    }
}
