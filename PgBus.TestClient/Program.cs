using HtmlAgilityPack;
using Newtonsoft.Json;
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
            var lineId = "8e5d4897ea68934df051b2aa865b21b014f5e61c";

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

                    }
                }
            }


        }
    }
}
