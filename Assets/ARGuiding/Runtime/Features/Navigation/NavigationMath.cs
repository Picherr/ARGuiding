using System;
using UnityEngine;

public static class NavigationMath
{
    /// <summary>
    /// Calculates the initial bearing from one WGS/GCJ latitude-longitude pair to another.
    /// The returned angle is clockwise from true north in degrees.
    /// </summary>
    public static float CalculateBearing(double fromLatitude, double fromLongitude,
        double toLatitude, double toLongitude)
    {
        double fromLatitudeRadians = fromLatitude * Math.PI / 180d;
        double toLatitudeRadians = toLatitude * Math.PI / 180d;
        double longitudeDeltaRadians = (toLongitude - fromLongitude) * Math.PI / 180d;

        double y = Math.Sin(longitudeDeltaRadians) * Math.Cos(toLatitudeRadians);
        double x = Math.Cos(fromLatitudeRadians) * Math.Sin(toLatitudeRadians) -
                   Math.Sin(fromLatitudeRadians) * Math.Cos(toLatitudeRadians) *
                   Math.Cos(longitudeDeltaRadians);

        return (float)((Math.Atan2(y, x) * 180d / Math.PI + 360d) % 360d);
    }

    /// <summary>
    /// Converts a target bearing into a direction relative to the current camera heading.
    /// </summary>
    public static Vector3 GetLocalDirection(float deviceHeading, float targetBearing, float length = 5f)
    {
        float relativeBearing = Mathf.DeltaAngle(deviceHeading, targetBearing) * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(relativeBearing) * length, 1f,
            Mathf.Cos(relativeBearing) * length);
    }
}
