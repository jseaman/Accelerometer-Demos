using System;
using WiimoteLib;

namespace WiiMoteIRDemo;

internal sealed class WiimoteIrConnection : IDisposable
{
    private readonly Wiimote wiimote = new();

    public bool IsConnected { get; private set; }
    public string Status { get; private set; } = "Not connected - press R to retry";

    public void TryConnect()
    {
        if (IsConnected)
            return;

        try
        {
            wiimote.Connect();
            // Extended IR mode provides all camera fields. This demo visualizes
            // the first two blobs produced by a normal two-LED sensor bar.
            wiimote.SetReportType(InputReport.IRAccel, IRSensitivity.Maximum, true);
            wiimote.SetLEDs(1);
            IsConnected = true;
            Status = "Connected - IR camera active";
        }
        catch (Exception ex)
        {
            IsConnected = false;
            Status = "Connection failed: " + ex.Message;
        }
    }

    public bool TryGetIrState(out IRState state)
    {
        if (!IsConnected)
        {
            state = default;
            return false;
        }

        state = wiimote.WiimoteState.IRState;
        return state.IRSensors is { Length: >= 2 };
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
    }
}
