using System;
using System.Collections.Generic;
using System.Text;

namespace PGBus.Models
{
    public class BusStopDescription
    {
        public string Code { get; set; }
        public string LineId { get; set; }
        public string Description { get; set; }
        public string FullDescription { get; set; }
    }
}
