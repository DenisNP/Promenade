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
        
        public static double Distance(GeoPoint p1, GeoPoint p2)
        {
            var p1Lat = p1.Lat * DegToRadFactor;
            var p2Lat = p2.Lat * DegToRadFactor;
            var diff = (p2.Lng - p1.Lng) * DegToRadFactor;
            var d3 = Math.Pow(Math.Sin((p2Lat - p1Lat) / 2.0), 2.0) 
                     + Math.Cos(p1Lat) * Math.Cos(p2Lat) * Math.Pow(Math.Sin(diff / 2.0), 2.0);

            return EarthRadiusKm * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
        }
    }
}