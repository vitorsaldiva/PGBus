using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;
using Newtonsoft.Json;
using PGBus.Models;

namespace PGBus.Services
{
    public class PiracicabanaService : IPiracicabanaService
    {
        //URL principal
        private const string url = "https://quantotempofaltapg.piracicabana.com.br";

        //URL secundária, com informações detalhadas dos pontos de parada
        private const string url_busStop = "https://geopg.piracicabana.com.br/";

        private static HtmlWeb webPage = new HtmlWeb();
        private static HtmlDocument docPage = new HtmlDocument();

        public List<BusStop> LoadBusStops(string lineId)
        {
            var param = new List<KeyValuePair<string, string>>();
            param.Add(new KeyValuePair<string, string>("idLinha", $"{lineId}"));

            var latLngBusStop = new List<BusStop>();

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

                    if (scriptNode != null)
                    {
                        var jsonPontos = scriptNode?.InnerText
                                .Replace("\n", string.Empty)
                                .Replace("\r", string.Empty)
                                .Replace("\t", string.Empty)
                                .Split(';')
                                .Where(l => l.StartsWith("var") && l.Contains("pontos"))
                                .FirstOrDefault()?.Trim()?.Replace("var pontos = ", "");

                        latLngBusStop = JsonConvert.DeserializeObject<List<BusStop>>(jsonPontos);
                    }
                }
            }

            return latLngBusStop;
        }

        public List<Vehicle> LoadVehicles(string lineId)
        {
            var doc = webPage.LoadFromWebAsync(url + $"/pg_mapaLinha.php?idLinha={lineId}").Result;

            var scriptNode = doc.DocumentNode.SelectNodes("//script[last()]").Where(n => !string.IsNullOrEmpty(n?.InnerHtml))?.FirstOrDefault();

            var vehicles = new List<Vehicle>();

            if (scriptNode != null)
            {
                var linhaId = Regex.Match(scriptNode.InnerText
                        .Replace("\n", string.Empty)
                        .Replace("\r", string.Empty)
                        .Replace("\t", string.Empty)
                        .Replace("{", string.Empty)
                        .Replace("}", string.Empty)
                        .Split(';')
                        .Where(l => l.Contains("var_linha") && !l.EndsWith(")"))
                        .FirstOrDefault(), @"\= \d+$").Value?.Replace("=", "").Trim();

                var vehicle_url = url + $"/parts/update_bus.php";

                var param = new List<KeyValuePair<string, string>>();
                param.Add(new KeyValuePair<string, string>("linha_id", $"{linhaId}"));

                using (var response = new HttpClient().PostAsync(vehicle_url, new FormUrlEncodedContent(param)).Result)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var json = response.Content.ReadAsStringAsync().Result;
                        json = json.Insert(0, "[").Insert((json.Length + 1), "]");
                        vehicles = JsonConvert.DeserializeObject<List<Vehicle>>(json);
                    }
                } 
            }

            return vehicles;
        }

        public List<BusStopDescription> LoadLinesId()
        {
            var doc = webPage.Load(url + "/pg_FindLines.php");

            var linhas = new List<BusStopDescription>();

            doc.DocumentNode.SelectNodes("//*[@id='middle']/a").ToList().ForEach(node =>
            {
                var linha = new BusStopDescription();
                var linhaCodigo = node.SelectNodes(".//span/strong")?.First()?.InnerText.Split(' ')[1];
                var linhaDescricao = node.SelectNodes(".//span[2]")?.First()?.InnerText;
                var link = node.Attributes["href"].Value;

                Uri myUri = new Uri($"{url}/{link}");
                string idLinha = HttpUtility.ParseQueryString(myUri.Query).Get("idLinha");

                linha.Code = linhaCodigo;
                linha.LineId = idLinha;
                linha.Description = linhaDescricao.Length > 70 ? 
                                linhaDescricao.Substring(0, 70) : linhaDescricao;


                linhas.Add(linha);
            });

            return linhas;
        }
    }
}
