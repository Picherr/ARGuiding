using NUnit.Framework;

public class AmapResponseParserTests
{
    [Test]
    public void TryParseConvertedLocation_ReturnsCoordinateForSuccessfulResponse()
    {
        const string response = "{\"status\":\"1\",\"locations\":\"113.295082,23.138099\"}";

        LatLng location;
        string error;
        bool result = AmapResponseParser.TryParseConvertedLocation(response, out location, out error);

        Assert.That(result, Is.True);
        Assert.That(error, Is.Empty);
        Assert.That(location.Longitude, Is.EqualTo(113.295082d).Within(0.000001d));
        Assert.That(location.Latitude, Is.EqualTo(23.138099d).Within(0.000001d));
    }

    [TestCase("")]
    [TestCase("not-json")]
    [TestCase("{\"status\":\"1\",\"locations\":\"invalid\"}")]
    public void TryParseConvertedLocation_RejectsInvalidResponse(string response)
    {
        LatLng location;
        string error;
        bool result = AmapResponseParser.TryParseConvertedLocation(response, out location, out error);

        Assert.That(result, Is.False);
        Assert.That(location, Is.Null);
        Assert.That(error, Is.Not.Empty);
    }

    [Test]
    public void TryParseConvertedLocation_IncludesAmapBusinessError()
    {
        const string response = "{\"status\":\"0\",\"info\":\"INVALID_USER_KEY\"}";

        LatLng location;
        string error;
        bool result = AmapResponseParser.TryParseConvertedLocation(response, out location, out error);

        Assert.That(result, Is.False);
        Assert.That(error, Does.Contain("INVALID_USER_KEY"));
    }

    [Test]
    public void TryParseWalkingRoute_CombinesStepsAndSkipsRepeatedJoinPoint()
    {
        const string response = "{\"status\":\"1\",\"route\":{\"paths\":[{\"distance\":\"120\",\"steps\":[" +
                                "{\"instruction\":\"向东步行\",\"polyline\":\"113.1,23.1;113.2,23.2\"}," +
                                "{\"instruction\":\"向北步行\",\"polyline\":\"113.2,23.2;113.3,23.3\"}]}]}}";

        WalkingRouteData route;
        string error;
        bool result = AmapResponseParser.TryParseWalkingRoute(response, out route, out error);

        Assert.That(result, Is.True);
        Assert.That(error, Is.Empty);
        Assert.That(route.Instruction, Is.EqualTo("向东步行"));
        Assert.That(route.Distance, Is.EqualTo("120"));
        Assert.That(route.Waypoints, Has.Count.EqualTo(3));
        Assert.That(route.Waypoints[2].Longitude, Is.EqualTo(113.3d).Within(0.000001d));
        Assert.That(route.Waypoints[2].Latitude, Is.EqualTo(23.3d).Within(0.000001d));
    }

    [TestCase("")]
    [TestCase("not-json")]
    [TestCase("{\"status\":\"1\",\"route\":{\"paths\":[]}}")]
    [TestCase("{\"status\":\"1\",\"route\":{\"paths\":[{\"distance\":\"10\",\"steps\":[{\"instruction\":\"继续\",\"polyline\":\"invalid\"}]}]}}")]
    public void TryParseWalkingRoute_RejectsIncompleteResponse(string response)
    {
        WalkingRouteData route;
        string error;
        bool result = AmapResponseParser.TryParseWalkingRoute(response, out route, out error);

        Assert.That(result, Is.False);
        Assert.That(route, Is.Null);
        Assert.That(error, Is.Not.Empty);
    }

    [Test]
    public void TryParseWalkingRoute_IncludesAmapBusinessError()
    {
        const string response = "{\"status\":\"0\",\"info\":\"DAILY_QUERY_OVER_LIMIT\"}";

        WalkingRouteData route;
        string error;
        bool result = AmapResponseParser.TryParseWalkingRoute(response, out route, out error);

        Assert.That(result, Is.False);
        Assert.That(error, Does.Contain("DAILY_QUERY_OVER_LIMIT"));
    }
}
