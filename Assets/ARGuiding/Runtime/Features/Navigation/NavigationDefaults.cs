public static class NavigationDefaults
{
    public const double ParkCenterLongitude = 113.295082d;
    public const double ParkCenterLatitude = 23.138099d;
    public const float ArrivalDistanceMeters = 20f;
    public const float DirectionRefreshSeconds = 10f;

    public static LatLng CreateParkCenter()
    {
        return new LatLng(ParkCenterLongitude, ParkCenterLatitude);
    }
}
