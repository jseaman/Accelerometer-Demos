using System;
using System.Numerics;
using WiimoteLib;

namespace WiiMoteDemo_Raylib;

internal sealed class WiimoteConnection : IDisposable
{
    private readonly Wiimote wiimote = new();

    public bool IsConnected { get; private set; }
    public string Status { get; private set; } = "Wii Remote not connected. Press R to retry.";

    public void TryConnect()
    {
        if (IsConnected)
            return;

        try
        {
            wiimote.Connect();
            wiimote.SetReportType(InputReport.ButtonsAccel, true);
            wiimote.SetLEDs(1);
            IsConnected = true;
            Status = "Connected: Nintendo Wii Remote";
        }
        catch (Exception ex)
        {
            IsConnected = false;
            Status = "Not connected: " + ex.Message + "  (close Dolphin, wake remote, press R)";
        }
    }

    public bool TryGetAcceleration(out Vector3 acceleration)
    {
        if (!IsConnected)
        {
            acceleration = default;
            return false;
        }

        var values = wiimote.WiimoteState.AccelState.Values;
        acceleration = new Vector3(values.X, values.Y, values.Z);
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
