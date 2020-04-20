using PGBus.MapCustomization;
using System;
using System.Collections.Generic;
using System.Text;
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
