using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using PGBus.Models;

namespace PGBus.Services
{
    public class PiracicabanaService : IPiracicabanaService
    {
        const string url = "https://quantotempofaltapg.piracicabana.com.br";
        private static HtmlWeb webPage = new HtmlWeb();

        public List<BusStop> LoadBusStops(string lineId)
        {
            var doc = webPage.Load(url + $"/pg_mapaLinha.php?idLinha={lineId}");
            var scriptNode = doc.DocumentNode.SelectNodes("//script[last()]").Where(n => !string.IsNullOrEmpty(n.InnerHtml))?.First();

            var latLngBusStop = new List<BusStop>();

            scriptNode?.InnerText
                .Replace("\n", string.Empty)
                .Replace("\r", string.Empty)
                .Replace("\t", string.Empty)
                .Replace("{", string.Empty)
                .Replace("}", string.Empty)
                .Split(';')
                .Where(l => !l.StartsWith("function") && l.Contains("ExibePontosLinha"))
                .ToList().ForEach(lc =>
                {
                    var coords = Regex.Match(lc, @"(\-?\d+(\.\d+)?),\s*(\-?\d+(\.\d+)?)")?.Value.Split(',');
                    var stop = new BusStop { Lat = Convert.ToDouble(coords[0]), Lng = Convert.ToDouble(coords[1]) };
                    latLngBusStop.Add(stop);
                });

            return latLngBusStop;
        }

        public List<Vehicle> LoadVehicles(string lineId)
        {
            throw new NotImplementedException();
        }
    }
}
