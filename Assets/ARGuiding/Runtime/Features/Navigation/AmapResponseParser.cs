using System;
using System.Collections.Generic;
using System.Globalization;
using LitJson;

public sealed class WalkingRouteData
{
    public string Instruction { get; private set; }
    public string Distance { get; private set; }
    public List<LatLng> Waypoints { get; private set; }

    public WalkingRouteData(string instruction, string distance, List<LatLng> waypoints)
    {
        Instruction = instruction;
        Distance = distance;
        Waypoints = waypoints;
    }
}

public static class AmapResponseParser
{
    public static bool TryParseConvertedLocation(string responseText, out LatLng location, out string error)
    {
        location = null;

        JsonData root;
        if (!TryParseRoot(responseText, out root, out error))
        {
            return false;
        }

        if (!IsSuccessful(root))
        {
            error = "高德坐标转换失败。" + GetApiError(root);
            return false;
        }

        JsonData locations;
        if (!TryGet(root, "locations", out locations))
        {
            error = "高德坐标转换未返回坐标。";
            return false;
        }

        string[] parts = locations.ToString().Split(',');
        double longitude;
        double latitude;
        if (parts.Length != 2 ||
            !double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out longitude) ||
            !double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out latitude))
        {
            error = "高德坐标转换返回了无效坐标。";
            return false;
        }

        location = new LatLng(longitude, latitude);
        error = string.Empty;
        return true;
    }

    public static bool TryParseWalkingRoute(string responseText, out WalkingRouteData routeData, out string error)
    {
        routeData = null;

        JsonData root;
        if (!TryParseRoot(responseText, out root, out error))
        {
            return false;
        }

        if (!IsSuccessful(root))
        {
            error = "高德步行路线规划失败。" + GetApiError(root);
            return false;
        }

        JsonData route;
        JsonData paths;
        JsonData firstPath;
        JsonData steps;
        JsonData firstStep;
        if (!TryGet(root, "route", out route) || !TryGet(route, "paths", out paths) ||
            !TryGet(paths, 0, out firstPath) || !TryGet(firstPath, "steps", out steps) ||
            !TryGet(steps, 0, out firstStep))
        {
            error = "未获取到可用的步行路线。";
            return false;
        }

        JsonData instruction;
        JsonData distance;
        if (!TryGet(firstStep, "instruction", out instruction) ||
            !TryGet(firstPath, "distance", out distance))
        {
            error = "步行路线缺少导航说明或距离。";
            return false;
        }

        List<LatLng> waypoints = new List<LatLng>();
        int stepCount = GetCount(steps);
        for (int i = 0; i < stepCount; i++)
        {
            JsonData step;
            JsonData polylineData;
            if (!TryGet(steps, i, out step) || !TryGet(step, "polyline", out polylineData))
            {
                continue;
            }

            string[] polyline = polylineData.ToString().Split(';');
            for (int j = i == 0 ? 0 : 1; j < polyline.Length; j++)
            {
                LatLng waypoint;
                if (TryParseCoordinate(polyline[j], out waypoint))
                {
                    waypoints.Add(waypoint);
                }
            }
        }

        if (waypoints.Count == 0)
        {
            error = "步行路线未返回有效坐标点。";
            return false;
        }

        routeData = new WalkingRouteData(instruction.ToString(), distance.ToString(), waypoints);
        error = string.Empty;
        return true;
    }

    private static bool TryParseRoot(string responseText, out JsonData root, out string error)
    {
        root = null;
        if (string.IsNullOrWhiteSpace(responseText))
        {
            error = "服务返回了空响应。";
            return false;
        }

        try
        {
            root = JsonMapper.ToObject(responseText);
            error = string.Empty;
            return root != null;
        }
        catch (Exception)
        {
            error = "服务返回了无法解析的数据。";
            return false;
        }
    }

    private static bool IsSuccessful(JsonData root)
    {
        JsonData status;
        return TryGet(root, "status", out status) && status.ToString() == "1";
    }

    private static string GetApiError(JsonData root)
    {
        JsonData info;
        if (!TryGet(root, "info", out info))
        {
            return string.Empty;
        }

        string message = info.ToString();
        return string.IsNullOrWhiteSpace(message) ? string.Empty : "（" + message + "）";
    }

    private static bool TryParseCoordinate(string value, out LatLng coordinate)
    {
        coordinate = null;
        string[] parts = value.Split(',');
        double longitude;
        double latitude;
        if (parts.Length != 2 ||
            !double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out longitude) ||
            !double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out latitude))
        {
            return false;
        }

        coordinate = new LatLng(longitude, latitude);
        return true;
    }

    private static bool TryGet(JsonData parent, string key, out JsonData value)
    {
        value = null;
        if (parent == null)
        {
            return false;
        }

        try
        {
            value = parent[key];
            return value != null;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static bool TryGet(JsonData parent, int index, out JsonData value)
    {
        value = null;
        if (parent == null || index < 0)
        {
            return false;
        }

        try
        {
            if (index >= parent.Count)
            {
                return false;
            }

            value = parent[index];
            return value != null;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static int GetCount(JsonData value)
    {
        try
        {
            return value == null ? 0 : value.Count;
        }
        catch (Exception)
        {
            return 0;
        }
    }
}
