using PGBus.MapCustomization;
using System;
using System.Collections.Generic;
using System.Text;

namespace PGBus.Models
{
    public class BusStopAndRoute
    {
        public List<BusStop> BusStops { get; set; }
        public List<CustomPosition> RotaIda { get; set; }
        public List<CustomPosition> RotaVolta { get; set; }
    }
}
