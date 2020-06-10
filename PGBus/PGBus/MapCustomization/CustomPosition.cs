using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms.GoogleMaps;

namespace PGBus.MapCustomization
{
    public class CustomPosition
    {
        public CustomPosition(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        [JsonProperty(PropertyName = "lat")]
        public double Latitude
        {
            get;
            set;
        }
        [JsonProperty(PropertyName = "lng")]
        public double Longitude
        {
            get;
            set;
        }
    }

    public static class CustomPositionExtensions
    {
        public static List<Position> ConvertToPositionList(this IEnumerable<CustomPosition> positions)
        {
            var convertedPosition = new List<Position>();

            positions.ToList().ForEach(p =>
            {
                convertedPosition.Add(new Position(p.Latitude, p.Longitude));
            });

            return convertedPosition;
        }
    }
}
