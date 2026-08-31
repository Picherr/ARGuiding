using NUnit.Framework;
using UnityEngine;

public class LocationTests
{
    [Test]
    public void LatLngToTileXY_MapsPrimeMeridianAndEquatorToCenter()
    {
        TileInfo tile = Location.LatLngToTileXY(new LatLng(0d, 0d), 1);

        Assert.That(tile.TileX, Is.EqualTo(1));
        Assert.That(tile.TileY, Is.EqualTo(1));
        Assert.That(tile.PixelX, Is.EqualTo(0));
        Assert.That(tile.PixelY, Is.EqualTo(0));
    }

    [Test]
    public void GlobalPixelToLatLng_MapsWorldCenterToZero()
    {
        LatLng coordinate = Location.GlobalPixelToLatLng(256d, 256d, 1);

        Assert.That(coordinate.Longitude, Is.EqualTo(0d).Within(0.000001d));
        Assert.That(coordinate.Latitude, Is.EqualTo(0d).Within(0.000001d));
    }

    [Test]
    public void TileAndPixelConversion_RoundTripsParkCoordinate()
    {
        const int zoom = 18;
        LatLng original = new LatLng(113.295128d, 23.139692d);
        TileInfo tile = Location.LatLngToTileXY(original, zoom);
        double globalPixelX = tile.TileX * LocationMap.TileWidthAndHeigth + tile.PixelX;
        double globalPixelY = tile.TileY * LocationMap.TileWidthAndHeigth + tile.PixelY;

        LatLng converted = Location.GlobalPixelToLatLng(globalPixelX, globalPixelY, zoom);

        Assert.That(converted.Longitude, Is.EqualTo(original.Longitude).Within(0.00001d));
        Assert.That(converted.Latitude, Is.EqualTo(original.Latitude).Within(0.00001d));
    }

    [Test]
    public void Conversion_MapsConfiguredBoundsToRouteViewport()
    {
        LatLng previousTopLeft = Conversion.TopLeftCoord;
        LatLng previousBottomRight = Conversion.BottomRightCoord;
        try
        {
            Conversion.ConfigureMapBounds(new LatLng(10d, 20d), new LatLng(12d, 18d));

            Vector3 topLeft = Conversion.GetWorldPoint(new Vector2(10f, 20f));
            Vector3 bottomRight = Conversion.GetWorldPoint(new Vector2(12f, 18f));

            Assert.That(topLeft.x, Is.EqualTo(0f).Within(0.001f));
            Assert.That(topLeft.z, Is.EqualTo(2000f).Within(0.001f));
            Assert.That(bottomRight.x, Is.EqualTo(1080f).Within(0.001f));
            Assert.That(bottomRight.z, Is.EqualTo(920f).Within(0.001f));
        }
        finally
        {
            Conversion.ConfigureMapBounds(previousTopLeft, previousBottomRight);
        }
    }
}
