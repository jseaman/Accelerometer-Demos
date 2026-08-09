using System;
using System.Numerics;
using System.Threading;
using WiimoteLib;

namespace WiiMoteGyroPointerDemo;

internal readonly record struct MotionSample(
    Vector3 Acceleration,
    Vector3 RawGyroscope,
    Vector3 GyroDegreesPerSecond);

internal sealed class WiimoteConnection : IDisposable
{
    private const int RequiredCalibrationSamples = 120;
    private readonly Wiimote wiimote = new();
    private Vector3 gyroBias;
    private Vector3 gyroSum;
    private int gyroSamples;

    public bool IsConnected { get; private set; }
    public bool IsCalibrated => gyroSamples >= RequiredCalibrationSamples;
    public int CalibrationSamples => Math.Min(gyroSamples, RequiredCalibrationSamples);
    public int CalibrationTarget => RequiredCalibrationSamples;
    public string Status { get; private set; } = "Wii Remote Plus not connected. Press R to retry.";

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
        Status = "Calibrating gyro - point at screen center and keep still...";
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
        MotionPlusState motion = state.MotionPlusState;
        Vector3 raw = new(motion.RawValues.X, motion.RawValues.Y, motion.RawValues.Z);
        Vector3 acceleration = new(accel.X, accel.Y, accel.Z);

        if (!IsCalibrated)
        {
            gyroSum += raw;
            gyroSamples++;
            if (IsCalibrated)
            {
                gyroBias = gyroSum / RequiredCalibrationSamples;
                Status = "Connected: gyro pointer active";
            }

            sample = new MotionSample(acceleration, raw, Vector3.Zero);
            return true;
        }

        Vector3 delta = raw - gyroBias;
        Vector3 velocity = new(
            delta.X / (motion.YawFast ? 4.0f : 20.0f),
            delta.Y / (motion.RollFast ? 4.0f : 20.0f),
            delta.Z / (motion.PitchFast ? 4.0f : 20.0f));
        sample = new MotionSample(acceleration, raw, velocity);
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
