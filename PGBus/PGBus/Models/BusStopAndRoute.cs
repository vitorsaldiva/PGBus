using System.Collections.Generic;
using Xamarin.Forms.GoogleMaps;

namespace PGBus.Models
{
    public class BusStopAndRoute
    {
        public List<BusStop> BusStops { get; set; }
        public List<Position> RotaIda { get; set; }
        public List<Position> RotaVolta { get; set; }
    }
}
