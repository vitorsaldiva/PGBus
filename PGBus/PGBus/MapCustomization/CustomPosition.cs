using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.GoogleMaps;

namespace PGBus.MapCustomization
{
    public class CustomPosition
    {
        [JsonProperty(PropertyName = "lat")]
        public double Latitute 
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


        public static List<Position> ConvertToPosition(List<CustomPosition> positions)
        {
            var convertedPosition = new List<Position>();

            positions.ForEach(p =>
            {
                convertedPosition.Add(new Position(p.Latitute, p.Longitude));
            });

            return convertedPosition;
        }

    }
}
