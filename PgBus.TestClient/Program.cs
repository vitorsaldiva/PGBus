using HtmlAgilityPack;
using Newtonsoft.Json;
using PGBus.MapCustomization;
using PGBus.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace PgBus.TestClient
{
    class Program
    {
        private const string url_busStop = "https://geopg.piracicabana.com.br/";
        private static HtmlDocument docPage = new HtmlDocument();

        static void Main(string[] args)
        {
            var lineId = "8400d3cd4a790ffbff8f3982c3b34fc0ee8f7e0c";

            var param = new List<KeyValuePair<string, string>>();
            param.Add(new KeyValuePair<string, string>("idLinha", $"{lineId}"));

            using (var response = new HttpClient().PostAsync($"{url_busStop}/consulta_linha.php", new FormUrlEncodedContent(param)).Result)
            {
                if (response.IsSuccessStatusCode)
                {
                    docPage.LoadHtml(response.Content.ReadAsStringAsync().Result);
                    var scriptNode = docPage
                                        .DocumentNode
                                        .SelectNodes("//body/div/script")
                                        .Where(n => !string.IsNullOrEmpty(n?.InnerHtml))?
                                        .FirstOrDefault();

                    var busStops = new List<BusStop>();

                    if (scriptNode != null)
                    {
                        var jsonPontos = scriptNode?.InnerText
                                .Replace("\n", string.Empty)
                                .Replace("\r", string.Empty)
                                .Replace("\t", string.Empty)
                                .Split(';')
                                .Where(l => l.StartsWith("var") && l.Contains("pontos"))
                                .FirstOrDefault().Trim().Replace("var pontos = ", "");

                        busStops = JsonConvert.DeserializeObject<List<BusStop>>(jsonPontos);

                        var jsonRotaIda = scriptNode?.InnerText
                                .Replace("\n", string.Empty)
                                .Replace("\r", string.Empty)
                                .Replace("\t", string.Empty)
                                .Split(';')
                                .Where(l => l.StartsWith("var") && l.Contains("latlngIda"))
                                .FirstOrDefault()?.Trim()?.Replace("var latlngIda = ", "");

                        var rotaLinha = JsonConvert.DeserializeObject<List<CustomPosition>>(jsonRotaIda);

                        var jsonRotaVolta = scriptNode?.InnerText
                                .Replace("\n", string.Empty)
                                .Replace("\r", string.Empty)
                                .Replace("\t", string.Empty)
                                .Split(';')
                                .Where(l => l.Trim().StartsWith("var latlngVolta ="))
                                .FirstOrDefault()?.Trim()?.Replace("var latlngVolta = ", "");

                        var rotaLinhaVolta = JsonConvert.DeserializeObject<List<CustomPosition>>(jsonRotaVolta);

                    }
                }
            }


        }
    }
}
