﻿using System.Linq;
using System.Text.Json.Serialization;
using ActionRepeater.Core.Extentions;
using ActionRepeater.Win32;

namespace ActionRepeater.Core.Action;

public sealed class MouseMovement
{
    public POINT MovPoint { get; set; }

    /// <summary>
    /// The actual measured delay duration.
    /// </summary>
    public int ActualDelay { get; set; }

    /// <summary>
    /// The delay duration compensated for the system timer.
    /// </summary>
    [JsonIgnore]
    public int DelayDuration => (int)(ActualDelay / 1.2);

    public MouseMovement(POINT movPoint, int actualDelay)
    {
        MovPoint = movPoint;
        ActualDelay = actualDelay;
    }

    public static POINT OffsetPointWithinScreens(POINT point, POINT offset)
    {
        var monitors = SystemInformation.MonitorRects;

        var ogMonitor = monitors.First(m => m.ContainsInclusive(point.x, point.y));

        point.x += offset.x;
        if (!monitors.Any(m => m.ContainsInclusive(point.x, point.y)))
        {
            point.x = offset.x < 0 ? ogMonitor.Left : ogMonitor.Right;
        }

        point.y += offset.y;
        if (!monitors.Any(m => m.ContainsInclusive(point.x, point.y)))
        {
            point.y = offset.y < 0 ? ogMonitor.Top : ogMonitor.Bottom;
        }

        return point;
    }
}
