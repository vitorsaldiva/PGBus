using Newtonsoft.Json;
using System.Collections.Generic;
using Xamarin.Forms.GoogleMaps;

namespace PGBus.Models
{
    public class BusStop
    {
        [JsonProperty(PropertyName = "Ponto")]
        public string Codigo { get; set; } = "";
        public double Lat { get; set; }
        public double Lng { get; set; }
        public int Sentido { get; set; }
        public string Conteudo { get; set; } = "";

        public List<Position> PositionsIda { get; set; }
        public List<Position> PositionsVolta { get; set; }
    }
}
