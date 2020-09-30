using System;
using Promenade.Geo.Models;

namespace Promenade.Geo
{
    public static class GeoUtils
    {
        private const double DegToRadFactor = Math.PI / 180;
        private const double RadToDegFactor = 180 / Math.PI;
        private const double EarthRadiusKm = 6371.01;
        
        public static GeoPoint FindPointAtDistanceFrom(GeoPoint startPoint, double initialBearingRadians, double distanceKilometres)
        {
            
            var distRatio = distanceKilometres / EarthRadiusKm;
            var distRatioSine = Math.Sin(distRatio);
            var distRatioCosine = Math.Cos(distRatio);

            var startLatRad = startPoint.Lat * DegToRadFactor;
            var startLonRad = startPoint.Lng * DegToRadFactor;

            var startLatCos = Math.Cos(startLatRad);
            var startLatSin = Math.Sin(startLatRad);

            var endLatRads = Math.Asin(
                startLatSin * distRatioCosine + startLatCos * distRatioSine * Math.Cos(initialBearingRadians)
            );

            var endLonRads = startLonRad + Math.Atan2(
                Math.Sin(initialBearingRadians) * distRatioSine * startLatCos,
                distRatioCosine - startLatSin * Math.Sin(endLatRads)
            );
            
            return new GeoPoint
            {
                Lat = endLatRads * RadToDegFactor,
                Lng = endLonRads * RadToDegFactor
            };
        }
        
        public static GeoPoint MidPoint(GeoPoint posA, GeoPoint posB)
        {
            var posALng = posA.Lng * DegToRadFactor;
            var posALat = posA.Lat * DegToRadFactor;
            var posBLng = posB.Lng * DegToRadFactor;
            var posBLat = posB.Lat * DegToRadFactor;
            
            var dLng = posBLng - posALng;
            var bx = Math.Cos(posBLat) * Math.Cos(dLng);
            var by = Math.Cos(posBLat) * Math.Sin(dLng);

            var newLat = Math.Atan2(
                Math.Sin(posALat) + Math.Sin(posBLat),
                Math.Sqrt((Math.Cos(posALat) + bx) * (Math.Cos(posALat) + bx) + by * by)
            );

            var newLng = posA.Lng + Math.Atan2(by, Math.Cos(posALat) + bx);

            return new GeoPoint
            {
                Lat = newLat * RadToDegFactor,
                Lng = newLng * RadToDegFactor
            };
        }
    }
}