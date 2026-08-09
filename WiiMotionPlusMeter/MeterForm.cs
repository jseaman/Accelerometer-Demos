using System;
using System.Drawing;
using System.Numerics;
using System.Threading;
using System.Windows.Forms;
using AquaControls;
using WiimoteLib;

namespace WiiMotionPlusMeter;

internal sealed class MeterForm : Form
{
    private const int GyroCalibrationSamples = 120;
    private readonly Wiimote wiimote = new();
    private readonly Label statusLabel = new();
    private readonly TableLayoutPanel layout = new();
    private readonly GroupBox gyroGroup = new();
    private readonly System.Windows.Forms.Timer repaintTimer = new();
    private readonly AquaGauge accelX;
    private readonly AquaGauge accelY;
    private readonly AquaGauge accelZ;
    private readonly AquaGauge gyroYaw;
    private readonly AquaGauge gyroRoll;
    private readonly AquaGauge gyroPitch;

    private bool connected;
    private bool motionPlusVisible;
    private int updatePending;
    private int calibrationCount;
    private Vector3 calibrationSum;
    private Vector3 gyroBias;
    private int closing;

    public MeterForm()
    {
        Text = "Wii MotionPlus Meter";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        BackColor = Color.White;
        DoubleBuffered = true;
        ClientSize = new Size(1230, 377);

        accelX = CreateGauge("Acceleration X (g)", -3.0f, 3.0f);
        accelY = CreateGauge("Acceleration Y (g)", -3.0f, 3.0f);
        accelZ = CreateGauge("Acceleration Z (g)", -3.0f, 3.0f);
        gyroYaw = CreateGauge("Gyro Yaw (deg/s)", -500.0f, 500.0f);
        gyroRoll = CreateGauge("Gyro Roll (deg/s)", -500.0f, 500.0f);
        gyroPitch = CreateGauge("Gyro Pitch (deg/s)", -500.0f, 500.0f);

        statusLabel.Dock = DockStyle.Fill;
        statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        statusLabel.Padding = new Padding(12, 0, 0, 0);
        statusLabel.Font = new Font(Font.FontFamily, 10.0f, FontStyle.Bold);
        statusLabel.Text = "Connecting... Keep the Wii Remote still for gyro calibration.";

        layout.Dock = DockStyle.Fill;
        layout.ColumnCount = 1;
        layout.RowCount = 3;
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42.0f));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 325.0f));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 0.0f));
        layout.Controls.Add(statusLabel, 0, 0);
        layout.Controls.Add(CreateGaugeGroup("Wii Remote accelerometer", accelX, accelY, accelZ), 0, 1);

        gyroGroup.Text = "Wii MotionPlus gyroscopes";
        gyroGroup.Dock = DockStyle.Fill;
        gyroGroup.Padding = new Padding(8);
        gyroGroup.Controls.Add(CreateGaugeRow(gyroYaw, gyroRoll, gyroPitch));
        gyroGroup.Visible = false;
        layout.Controls.Add(gyroGroup, 0, 2);
        Controls.Add(layout);

        repaintTimer.Interval = 33;
        repaintTimer.Tick += (_, _) => RefreshAllGauges();
        repaintTimer.Start();

        Shown += OnShown;
        FormClosing += OnFormClosing;
        FormClosed += OnFormClosed;
    }

    private static AquaGauge CreateGauge(string text, float minimum, float maximum)
    {
        return new AquaGauge
        {
            BackColor = Color.Transparent,
            DialColor = Color.Lavender,
            DialText = text,
            Glossiness = 11.36364f,
            MinValue = minimum,
            MaxValue = maximum,
            RecommendedValue = 0.0f,
            ThresholdPercent = 20.0f,
            Value = 0.0f,
            AutoSize = false,
            MinimumSize = new Size(280, 280),
            Size = new Size(280, 280),
            Anchor = AnchorStyles.None
        };
    }

    private static GroupBox CreateGaugeGroup(string title, params AquaGauge[] gauges)
    {
        GroupBox group = new() { Text = title, Dock = DockStyle.Fill, Padding = new Padding(8) };
        group.Controls.Add(CreateGaugeRow(gauges));
        return group;
    }

    private static TableLayoutPanel CreateGaugeRow(params AquaGauge[] gauges)
    {
        TableLayoutPanel row = new() { Dock = DockStyle.Fill, ColumnCount = gauges.Length, RowCount = 1 };
        foreach (AquaGauge gauge in gauges)
        {
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100.0f / gauges.Length));
            row.Controls.Add(gauge);
        }
        return row;
    }

    private void OnShown(object? sender, EventArgs e)
    {
        try
        {
            wiimote.Connect();
            connected = true;
            wiimote.WiimoteChanged += OnWiimoteChanged;
            wiimote.InitializeMotionPlus();
            Thread.Sleep(750);

            if (wiimote.WiimoteState.ExtensionType == ExtensionType.MotionPlus)
            {
                wiimote.SetReportType(InputReport.ExtensionAccel, true);
                SetMotionPlusVisibility(true);
                statusLabel.Text = "MotionPlus detected - keep still while the gyroscopes calibrate.";
            }
            else
            {
                wiimote.SetReportType(InputReport.ButtonsAccel, true);
                SetMotionPlusVisibility(false);
                statusLabel.Text = "Wii Remote connected without MotionPlus; gyro gauges are hidden.";
            }

            wiimote.SetLEDs(1);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Could not connect to the Wii Remote.\n\nClose Dolphin, wake the remote, and try again.\n\n" + ex.Message,
                "Wii Remote not available", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
        }
    }

    private void OnWiimoteChanged(object? sender, WiimoteChangedEventArgs e)
    {
        if (Volatile.Read(ref closing) != 0 || IsDisposed || !IsHandleCreated)
            return;

        WiimoteState state = e.WiimoteState;
        Vector3 velocity = Vector3.Zero;
        bool isMotionPlus = state.ExtensionType == ExtensionType.MotionPlus;

        if (isMotionPlus)
        {
            MotionPlusState motion = state.MotionPlusState;
            Vector3 raw = new(motion.RawValues.X, motion.RawValues.Y, motion.RawValues.Z);
            if (calibrationCount < GyroCalibrationSamples)
            {
                calibrationSum += raw;
                calibrationCount++;
                if (calibrationCount == GyroCalibrationSamples)
                    gyroBias = calibrationSum / GyroCalibrationSamples;
            }
            else
            {
                Vector3 delta = raw - gyroBias;
                velocity = new Vector3(
                    delta.X / (motion.YawFast ? 4.0f : 20.0f),
                    delta.Y / (motion.RollFast ? 4.0f : 20.0f),
                    delta.Z / (motion.PitchFast ? 4.0f : 20.0f));
            }
        }

        if (Interlocked.Exchange(ref updatePending, 1) != 0)
            return;

        var acceleration = state.AccelState.Values;
        bool yawFast = isMotionPlus && state.MotionPlusState.YawFast;
        bool rollFast = isMotionPlus && state.MotionPlusState.RollFast;
        bool pitchFast = isMotionPlus && state.MotionPlusState.PitchFast;
        try
        {
            BeginInvoke((Action)(() =>
            {
                try
                {
                    if (Volatile.Read(ref closing) != 0)
                        return;

                    SetMotionPlusVisibility(isMotionPlus);
                    SetGaugeValue(accelX, acceleration.X, 1.2f);
                    SetGaugeValue(accelY, acceleration.Y, 1.2f);
                    SetGaugeValue(accelZ, acceleration.Z, 1.2f);

                    if (isMotionPlus)
                    {
                        ConfigureGyroScale(gyroYaw, yawFast);
                        ConfigureGyroScale(gyroRoll, rollFast);
                        ConfigureGyroScale(gyroPitch, pitchFast);
                        SetGaugeValue(gyroYaw, velocity.X, yawFast ? 900.0f : 250.0f);
                        SetGaugeValue(gyroRoll, velocity.Y, rollFast ? 900.0f : 250.0f);
                        SetGaugeValue(gyroPitch, velocity.Z, pitchFast ? 900.0f : 250.0f);
                        statusLabel.Text = calibrationCount < GyroCalibrationSamples
                            ? $"MotionPlus detected - keep still: calibrating {calibrationCount}/{GyroCalibrationSamples}"
                            : "MotionPlus detected - accelerometer and gyroscope streams active";
                    }
                }
                finally
                {
                    Interlocked.Exchange(ref updatePending, 0);
                }
            }));
        }
        catch (InvalidOperationException)
        {
            Interlocked.Exchange(ref updatePending, 0);
        }
    }

    private void SetMotionPlusVisibility(bool visible)
    {
        if (motionPlusVisible == visible)
            return;

        motionPlusVisible = visible;
        gyroGroup.Visible = visible;
        layout.RowStyles[2].Height = visible ? 325.0f : 0.0f;
        ClientSize = new Size(1230, visible ? 702 : 377);
        layout.PerformLayout();
        gyroGroup.PerformLayout();
        gyroGroup.Invalidate(true);
        layout.Invalidate(true);
        if (visible)
        {
            gyroYaw.Refresh();
            gyroRoll.Refresh();
            gyroPitch.Refresh();
        }
    }

    private static void ConfigureGyroScale(AquaGauge gauge, bool fast)
    {
        float limit = fast ? 2000.0f : 500.0f;
        if (gauge.MaxValue == limit)
            return;
        gauge.Value = 0.0f;
        gauge.MinValue = -limit;
        gauge.MaxValue = limit;
    }

    private static void SetGaugeValue(AquaGauge gauge, float value, float warning)
    {
        gauge.Value = Math.Clamp(value, gauge.MinValue, gauge.MaxValue);
        gauge.DialColor = Math.Abs(value) > warning ? Color.IndianRed : Color.Lavender;
        gauge.Refresh();
    }

    private void RefreshAllGauges()
    {
        if (Volatile.Read(ref closing) != 0)
            return;

        accelX.Refresh();
        accelY.Refresh();
        accelZ.Refresh();
        if (motionPlusVisible)
        {
            gyroYaw.Refresh();
            gyroRoll.Refresh();
            gyroPitch.Refresh();
        }
    }

    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        if (Interlocked.Exchange(ref closing, 1) != 0)
            return;
        repaintTimer.Stop();
        if (connected)
            wiimote.WiimoteChanged -= OnWiimoteChanged;
    }

    private void OnFormClosed(object? sender, FormClosedEventArgs e)
    {
        if (!connected)
            return;
        try
        {
            wiimote.SetLEDs(0);
            wiimote.Disconnect();
        }
        catch
        {
            // The remote may already be disconnected.
        }
        connected = false;
    }
}
