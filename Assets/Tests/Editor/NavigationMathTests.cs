using NUnit.Framework;
using UnityEngine;

public class NavigationMathTests
{
    [TestCase(1d, 0d, 0f)]
    [TestCase(0d, 1d, 90f)]
    [TestCase(-1d, 0d, 180f)]
    [TestCase(0d, -1d, 270f)]
    public void CalculateBearing_ReturnsCardinalDirection(double latitude, double longitude,
        float expectedBearing)
    {
        float bearing = NavigationMath.CalculateBearing(0d, 0d, latitude, longitude);

        Assert.That(bearing, Is.EqualTo(expectedBearing).Within(0.001f));
    }

    [Test]
    public void GetLocalDirection_PointsRightWhenTargetIsEast()
    {
        Vector3 direction = NavigationMath.GetLocalDirection(0f, 90f, 5f);

        Assert.That(direction.x, Is.EqualTo(5f).Within(0.001f));
        Assert.That(direction.z, Is.EqualTo(0f).Within(0.001f));
    }

    [Test]
    public void GetLocalDirection_UsesShortestTurnAcrossNorth()
    {
        Vector3 direction = NavigationMath.GetLocalDirection(350f, 10f, 5f);

        Assert.That(direction.x, Is.GreaterThan(0f));
        Assert.That(direction.z, Is.GreaterThan(0f));
    }
}
