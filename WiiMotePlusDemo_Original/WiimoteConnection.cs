using System;
using System.Numerics;
using System.Threading;
using WiimoteLib;

namespace WiiMotePlusDemo_Original;

internal readonly record struct MotionSample(Vector3 Acceleration, Vector3 GyroDegreesPerSecond);

internal sealed class WiimoteConnection : IDisposable
{
    private const int CalibrationSamples = 120;
    private readonly Wiimote wiimote = new();
    private Vector3 gyroBias;
    private Vector3 gyroSum;
    private int gyroSamples;

    public bool IsConnected { get; private set; }
    public bool IsCalibrated => gyroSamples >= CalibrationSamples;
    public string Status { get; private set; } = "Wii Remote not connected. Press R to retry.";

    public void TryConnect()
    {
        if (IsConnected)
            return;

        try
        {
            wiimote.Connect();
            wiimote.InitializeMotionPlus();
            Thread.Sleep(500);
            wiimote.SetReportType(InputReport.ExtensionAccel, true);
            wiimote.SetLEDs(1);
            IsConnected = true;
            StartCalibration();
        }
        catch (Exception ex)
        {
            IsConnected = false;
            Status = "Not connected: " + ex.Message + "  (close Dolphin, wake remote, press R)";
        }
    }

    public void StartCalibration()
    {
        gyroBias = Vector3.Zero;
        gyroSum = Vector3.Zero;
        gyroSamples = 0;
        Status = "MotionPlus calibrating - keep the Wii Remote still...";
    }

    public bool TryGetMotion(out MotionSample sample)
    {
        if (!IsConnected)
        {
            sample = default;
            return false;
        }

        WiimoteState state = wiimote.WiimoteState;
        if (state.ExtensionType != ExtensionType.MotionPlus)
        {
            Status = $"Connected, waiting for MotionPlus (reported: {state.ExtensionType})";
            sample = default;
            return false;
        }

        var accel = state.AccelState.Values;
        var motionPlus = state.MotionPlusState;
        Vector3 raw = new(motionPlus.RawValues.X, motionPlus.RawValues.Y, motionPlus.RawValues.Z);

        if (!IsCalibrated)
        {
            gyroSum += raw;
            gyroSamples++;
            if (IsCalibrated)
            {
                gyroBias = gyroSum / CalibrationSamples;
                Status = "Connected: Wii Remote Plus + MotionPlus gyro";
            }

            sample = new MotionSample(new Vector3(accel.X, accel.Y, accel.Z), Vector3.Zero);
            return true;
        }

        // WiimoteLib exposes MotionPlus as X=yaw, Y=roll, Z=pitch.
        // Slow mode is about 20 raw units/(degree/sec), fast mode about 4.
        Vector3 delta = raw - gyroBias;
        Vector3 gyro = new(
            delta.X / (motionPlus.YawFast ? 4.0f : 20.0f),
            delta.Y / (motionPlus.RollFast ? 4.0f : 20.0f),
            delta.Z / (motionPlus.PitchFast ? 4.0f : 20.0f));

        sample = new MotionSample(new Vector3(accel.X, accel.Y, accel.Z), gyro);
        return true;
    }

    public void Dispose()
    {
        if (!IsConnected)
            return;

        try
        {
            wiimote.SetLEDs(0);
            wiimote.Disconnect();
        }
        catch
        {
            // The remote may already be asleep or disconnected.
        }
        finally
        {
            IsConnected = false;
        }
    }
}
