using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading;

namespace WiiMoteGyroPointerDemo;

internal static class NoiseSampler
{
    private const int DurationSeconds = 60;

    private static int Main()
    {
        using WiimoteConnection remote = new();
        remote.TryConnect();
        if (!remote.IsConnected)
        {
            Console.WriteLine(remote.Status);
            return 1;
        }

        Console.WriteLine("Keep the Wii Remote flat and completely still.");
        Console.WriteLine("Calibrating gyro bias...");
        while (!remote.IsCalibrated)
        {
            remote.TryGetMotion(out _);
            Thread.Sleep(10);
        }

        Console.WriteLine($"Sampling stationary gyro noise for {DurationSeconds} seconds...");
        List<float> yaw = new();
        List<float> roll = new();
        List<float> pitch = new();
        Stopwatch timer = Stopwatch.StartNew();
        int lastReportedSecond = -1;
        while (timer.Elapsed.TotalSeconds < DurationSeconds)
        {
            if (remote.TryGetMotion(out MotionSample sample) && IsFinite(sample.GyroDegreesPerSecond))
            {
                yaw.Add(sample.GyroDegreesPerSecond.X);
                roll.Add(sample.GyroDegreesPerSecond.Y);
                pitch.Add(sample.GyroDegreesPerSecond.Z);
            }

            int elapsedSecond = (int)timer.Elapsed.TotalSeconds;
            if (elapsedSecond != lastReportedSecond && elapsedSecond % 10 == 0)
            {
                Console.WriteLine($"  {elapsedSecond,2}/{DurationSeconds} seconds");
                lastReportedSecond = elapsedSecond;
            }
            Thread.Sleep(10);
        }

        Console.WriteLine();
        Console.WriteLine($"Samples: {yaw.Count}");
        PrintStatistics("Yaw   X", yaw);
        PrintStatistics("Roll  Y", roll);
        PrintStatistics("Pitch Z", pitch);
        return 0;
    }

    private static void PrintStatistics(string label, List<float> values)
    {
        if (values.Count == 0)
        {
            Console.WriteLine($"{label}: no samples");
            return;
        }

        double sum = 0.0;
        double sumSquares = 0.0;
        float minimum = float.PositiveInfinity;
        float maximum = float.NegativeInfinity;
        List<float> absolute = new(values.Count);
        foreach (float value in values)
        {
            sum += value;
            sumSquares += value * value;
            minimum = MathF.Min(minimum, value);
            maximum = MathF.Max(maximum, value);
            absolute.Add(MathF.Abs(value));
        }

        absolute.Sort();
        double mean = sum / values.Count;
        double variance = Math.Max(0.0, sumSquares / values.Count - mean * mean);
        Console.WriteLine(
            $"{label}: mean={mean,9:0.0000}  sigma={Math.Sqrt(variance),8:0.0000}  " +
            $"min={minimum,9:0.0000}  max={maximum,9:0.0000}  " +
            $"|v| p95={Percentile(absolute, 0.95f),7:0.0000}  p99={Percentile(absolute, 0.99f),7:0.0000}");
    }

    private static float Percentile(List<float> sorted, float fraction)
    {
        int index = Math.Clamp((int)MathF.Ceiling(fraction * sorted.Count) - 1, 0, sorted.Count - 1);
        return sorted[index];
    }

    private static bool IsFinite(Vector3 value) =>
        float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Z);
}
