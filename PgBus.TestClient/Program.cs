using HtmlAgilityPack;
using Newtonsoft.Json;
using PGBus.MapCustomization;
using PGBus.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using Xamarin.Essentials;
using Xamarin.Forms.GoogleMaps;

namespace PgBus.TestClient
{
    class Program
    {
        private const string url_busStop = "https://geopg.piracicabana.com.br/";
        private static HtmlDocument docPage = new HtmlDocument();

        static void Main(string[] args)
        {

            //CultureInfo ci = new CultureInfo("pt-BR");
            //Thread.CurrentThread.CurrentCulture = ci;
            //Thread.CurrentThread.CurrentUICulture = ci;
            //Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator = ",";
            var lat = -24.0095191;
            var lng = -46.4064068;
            var housePosition = new Position(lat, lng);

            var busPosition = new Position(-24.00503, -46.41312);

            var busStopPosition = new Position(-24.001006, -46.412468);

            var nearestPlaces = NearestPosition(busPosition, busStopPosition);


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

        static List<Position> NearestPosition(Position startPlace, Position endPlace)
        {
            var vehicleRoute = new List<Position>();

            var rota = JsonConvert.DeserializeObject<List<Position>>(JsonRotaVolta());

            /*
             *  Utiliza a coordenada inicial (lat/lng veículo) para buscar, 
             *  dentro da lista de coordenadas da rota da linha, a coordenada 
             *  mais próxima para exibição no mapa
             */
            var initialRoutePoint = rota.OrderBy(p => Haversine(startPlace, p))
                .FirstOrDefault();

            
            /*
             *  Utiliza a coordenada final (lat/lng ponto do onibus) para buscar, 
             *  dentro da lista de coordenadas da rota da linha, a coordenada 
             *  mais próxima para exibição no mapa
             */
            var finalRoutePoint = rota.OrderBy(p => Haversine(endPlace, p))
                .FirstOrDefault();

            
            /*
             * Seleciona apenas as coordenadas que estejam entre o intervalo entre o ponto inicial e o final
             * para exibição do polyline
             */
            var positions = rota
                .Skip(rota.IndexOf(initialRoutePoint))
                .Take(rota.IndexOf(finalRoutePoint) - rota.IndexOf(initialRoutePoint) + 1)
                .ToList();


            // Inclui os pontos inicial e final para exibição correta do polyline
            vehicleRoute.Insert(0, startPlace);
            vehicleRoute.Add(endPlace);

            Console.WriteLine(JsonConvert.SerializeObject(vehicleRoute).Replace("Latitude", "lat").Replace("Longitude", "lng"));


            return vehicleRoute;
        }

        static string JsonPontos()
        {
            return @"[
                        {
                          'ponto': '4659',
                          'lat': -23.999152,
                          'lng': -46.414534,
                          'sentido': 1,
                          'conteudo': '<b>1400 - AV. TRABALHADORES (TERM. TUDE BASTOS)</b>'
                        },
                        {
                          'ponto': '4748',
                          'lat': -24.001501,
                          'lng': -46.41271,
                          'sentido': 1,
                          'conteudo': '<b>7318 - AV. PRESIDENTE COSTA E SILVA,1341</b>'
                        },
                        {
                          'ponto': '4773',
                          'lat': -24.003198,
                          'lng': -46.413029,
                          'sentido': 1,
                          'conteudo': '<b>7877 - AV. PRESIDENTE COSTA E SILVA, 1319 </b>'
                        },
                        {
                          'ponto': '4563',
                          'lat': -24.005891,
                          'lng': -46.413491,
                          'sentido': 1,
                          'conteudo': '<b>1257 - AV. PRESIDENTE COSTA E SILVA, 1163</b>'
                        },
                        {
                          'ponto': '4565',
                          'lat': -24.007507,
                          'lng': -46.413661,
                          'sentido': 1,
                          'conteudo': '<b>1259 - AV. PRESIDENTE COSTA E SILVA, 843</b>'
                        },
                        {
                          'ponto': '4566',
                          'lat': -24.010129,
                          'lng': -46.414273,
                          'sentido': 1,
                          'conteudo': '<b>1260 - AV. PRESIDENTE COSTA E SILVA, 699</b>'
                        },
                        {
                          'ponto': '4747',
                          'lat': -24.013193,
                          'lng': -46.414752,
                          'sentido': 1,
                          'conteudo': '<b>7317 - AV. PRESIDENTE COSTA E SILVA, S/N</b>'
                        },
                        {
                          'ponto': '4567',
                          'lat': -24.012578,
                          'lng': -46.413853,
                          'sentido': 1,
                          'conteudo': '<b>1261 - RUA PERNAMBUCO, 50</b>'
                        },
                        {
                          'ponto': '4568',
                          'lat': -24.011008,
                          'lng': -46.413548,
                          'sentido': 1,
                          'conteudo': '<b>1262 - RUA PERNAMBUCO, 300</b>'
                        },
                        {
                          'ponto': '4451',
                          'lat': -24.011021,
                          'lng': -46.410018,
                          'sentido': 1,
                          'conteudo': '<b>1120 - RUA JAÚ, 695</b>'
                        },
                        {
                          'ponto': '4450',
                          'lat': -24.011216,
                          'lng': -46.407549,
                          'sentido': 1,
                          'conteudo': '<b>1119 - RUA JAÚ, 297</b>'
                        },
                        {
                          'ponto': '4619',
                          'lat': -24.010021,
                          'lng': -46.406556,
                          'sentido': 1,
                          'conteudo': '<b>1321 - RUA RIO BRANCO, 537</b>'
                        },
                        {
                          'ponto': '4813',
                          'lat': -24.008697,
                          'lng': -46.405187,
                          'sentido': 1,
                          'conteudo': '<b>9897 - AV. MARECHAL MALLET, 1071</b>'
                        },
                        {
                          'ponto': '4811',
                          'lat': -24.010636,
                          'lng': -46.40348,
                          'sentido': 1,
                          'conteudo': '<b>9895 - AV. MARECHAL MALLET, 701</b>'
                        },
                        {
                          'ponto': '4693',
                          'lat': -24.010109,
                          'lng': -46.40223,
                          'sentido': 1,
                          'conteudo': '<b>1436 - RUA XIXOVA, 562</b>'
                        },
                        {
                          'ponto': '4694',
                          'lat': -24.007923,
                          'lng': -46.401797,
                          'sentido': 1,
                          'conteudo': '<b>1437 - RUA XIXOVA, 824</b>'
                        },
                        {
                          'ponto': '4441',
                          'lat': -24.007624,
                          'lng': -46.400718,
                          'sentido': 2,
                          'conteudo': '<b>1108 - RUA CYNTHIA GILFFRIDA, 1000</b>'
                        },
                        {
                          'ponto': '4440',
                          'lat': -24.007957,
                          'lng': -46.399106,
                          'sentido': 2,
                          'conteudo': '<b>1107 - RUA CYNTHIA GILFFRIDA, 642</b>'
                        },
                        {
                          'ponto': '4439',
                          'lat': -24.009423,
                          'lng': -46.396377,
                          'sentido': 2,
                          'conteudo': '<b>1105 - RUA CYNTHIA GILFFRIDA, 301</b>'
                        },
                        {
                          'ponto': '4607',
                          'lat': -24.011946,
                          'lng': -46.397863,
                          'sentido': 2,
                          'conteudo': '<b>1307 - RUA COSTA MACHADO, 553</b>'
                        },
                        {
                          'ponto': '4606',
                          'lat': -24.014403,
                          'lng': -46.399584,
                          'sentido': 2,
                          'conteudo': '<b>1306 - RUA COSTA MACHADO, 266</b>'
                        },
                        {
                          'ponto': '4809',
                          'lat': -24.012232,
                          'lng': -46.401757,
                          'sentido': 2,
                          'conteudo': '<b>9893 - AV. MARECHAL MALLET, 504</b>'
                        },
                        {
                          'ponto': '4812',
                          'lat': -24.009811,
                          'lng': -46.403897,
                          'sentido': 2,
                          'conteudo': '<b>9896 - AV. MARECHAL MALLET, S/N (PRAÇA C. SAITTA)</b>'
                        },
                        {
                          'ponto': '4814',
                          'lat': -24.008464,
                          'lng': -46.405055,
                          'sentido': 2,
                          'conteudo': '<b>9898 - AV. MARECHAL MALLET, 938</b>'
                        },
                        {
                          'ponto': '4713',
                          'lat': -24.009588,
                          'lng': -46.406589,
                          'sentido': 2,
                          'conteudo': '<b>6197 - AV. RIO BRANCO, 528 </b>'
                        },
                        {
                          'ponto': '4714',
                          'lat': -24.0117,
                          'lng': -46.406827,
                          'sentido': 2,
                          'conteudo': '<b>6198 - AV. RIO BRANCO, 191</b>'
                        },
                        {
                          'ponto': '4715',
                          'lat': -24.012506,
                          'lng': -46.40882,
                          'sentido': 2,
                          'conteudo': '<b>6199 - RUA BAHIA, 408 </b>'
                        },
                        {
                          'ponto': '4716',
                          'lat': -24.012281,
                          'lng': -46.411665,
                          'sentido': 2,
                          'conteudo': '<b>6200 - RUA BAHIA, 680</b>'
                        },
                        {
                          'ponto': '4717',
                          'lat': -24.012119,
                          'lng': -46.413494,
                          'sentido': 2,
                          'conteudo': '<b>6201 - RUA BAHIA, 848</b>'
                        },
                        {
                          'ponto': '4568',
                          'lat': -24.011008,
                          'lng': -46.413548,
                          'sentido': 2,
                          'conteudo': '<b>1262 - RUA PERNAMBUCO, 300</b>'
                        },
                        {
                          'ponto': '4569',
                          'lat': -24.008477,
                          'lng': -46.412963,
                          'sentido': 2,
                          'conteudo': '<b>1263 - RUA PERNAMBUCO, 550</b>'
                        },
                        {
                          'ponto': '4564',
                          'lat': -24.005006,
                          'lng': -46.413124,
                          'sentido': 2,
                          'conteudo': '<b>1258 - AV. PRESIDENTE COSTA E SILVA, 964</b>'
                        },
                        {
                          'ponto': '4562',
                          'lat': -24.001006,
                          'lng': -46.412468,
                          'sentido': 2,
                          'conteudo': '<b>1256 - AV. PRESIDENTE COSTA E SILVA, 1389 LO</b>'
                        },
                        {
                          'ponto': '4759',
                          'lat': -23.998386,
                          'lng': -46.414144,
                          'sentido': 2,
                          'conteudo': '<b>7503 - TERMINAL TUDE BASTOS</b>'
                        }
                ]";
        }

        static string JsonRotaVolta()
        {
            //Linha 33
            return @"[
  {
    'Latitude': -23.99860262291778,
    'Longitude': -46.41421886000571
  },
  {
    'Latitude': -23.99880574882944,
    'Longitude': -46.41423914525971
  },
  {
    'Latitude': -23.99882438038922,
    'Longitude': -46.41428491760113
  },
  {
    'Latitude': -23.99907763610049,
    'Longitude': -46.41489763804964
  },
  {
    'Latitude': -23.99909868883059,
    'Longitude': -46.41505788639512
  },
  {
    'Latitude': -23.99906624515392,
    'Longitude': -46.41516730989714
  },
  {
    'Latitude': -23.99906393190975,
    'Longitude': -46.41518003207703
  },
  {
    'Latitude': -23.99903845594628,
    'Longitude': -46.41528436059092
  },
  {
    'Latitude': -23.99903614269118,
    'Longitude': -46.41529708277746
  },
  {
    'Latitude': -23.99904087919186,
    'Longitude': -46.41540138068372
  },
  {
    'Latitude': -23.99904553590039,
    'Longitude': -46.41541155183796
  },
  {
    'Latitude': -23.99907346106126,
    'Longitude': -46.41545477101369
  },
  {
    'Latitude': -23.99910136898518,
    'Longitude': -46.41547763846104
  },
  {
    'Latitude': -23.999157165437,
    'Longitude': -46.4155004776555
  },
  {
    'Latitude': -23.99918505449618,
    'Longitude': -46.41550045143841
  },
  {
    'Latitude': -23.99923617649965,
    'Longitude': -46.4154927656413
  },
  {
    'Latitude': -23.99924779456671,
    'Longitude': -46.41549020989016
  },
  {
    'Latitude': -23.99929656069139,
    'Longitude': -46.41544436887378
  },
  {
    'Latitude': -23.99929134782531,
    'Longitude': -46.41533928571697
  },
  {
    'Latitude': -23.99918915123791,
    'Longitude': -46.41485173308077
  },
  {
    'Latitude': -23.99906779567674,
    'Longitude': -46.4142591126035
  },
  {
    'Latitude': -23.99899313011901,
    'Longitude': -46.41391320800582
  },
  {
    'Latitude': -23.99895347744637,
    'Longitude': -46.41374528775599
  },
  {
    'Latitude': -23.99890511409727,
    'Longitude': -46.41363269980701
  },
  {
    'Latitude': -23.99886157711417,
    'Longitude': -46.41354280912329
  },
  {
    'Latitude': -23.99880357825637,
    'Longitude': -46.4134259988993
  },
  {
    'Latitude': -23.9987828740637,
    'Longitude': -46.41332455380672
  },
  {
    'Latitude': -23.99877662116968,
    'Longitude': -46.41322942209563
  },
  {
    'Latitude': -23.99880442539141,
    'Longitude': -46.41315614822556
  },
  {
    'Latitude': -23.9988470003602,
    'Longitude': -46.41310855157396
  },
  {
    'Latitude': -23.9989681916934,
    'Longitude': -46.41305375080449
  },
  {
    'Latitude': -23.99910336430188,
    'Longitude': -46.41300225239269
  },
  {
    'Latitude': -23.99924031821886,
    'Longitude': -46.41301034902724
  },
  {
    'Latitude': -23.99930005941344,
    'Longitude': -46.41305152551348
  },
  {
    'Latitude': -23.99938345682676,
    'Longitude': -46.41313201855567
  },
  {
    'Latitude': -23.99946746854695,
    'Longitude': -46.41318732441879
  },
  {
    'Latitude': -23.99953253041529,
    'Longitude': -46.4132017635789
  },
  {
    'Latitude': -23.99962781113213,
    'Longitude': -46.41321454567389
  },
  {
    'Latitude': -23.99973416692279,
    'Longitude': -46.413178705474
  },
  {
    'Latitude': -23.99984770464654,
    'Longitude': -46.41313539805432
  },
  {
    'Latitude': -23.99995066720186,
    'Longitude': -46.41305624566305
  },
  {
    'Latitude': -24.00002744878579,
    'Longitude': -46.4129583432743
  },
  {
    'Latitude': -24.0001303516305,
    'Longitude': -46.41281856376031
  },
  {
    'Latitude': -24.00020835604784,
    'Longitude': -46.41272592547907
  },
  {
    'Latitude': -24.00029690379478,
    'Longitude': -46.41265981938443
  },
  {
    'Latitude': -24.00041928990494,
    'Longitude': -46.41259967786441
  },
  {
    'Latitude': -24.00057126952555,
    'Longitude': -46.41259114502419
  },
  {
    'Latitude': -24.00070848482523,
    'Longitude': -46.41259244609758
  },
  {
    'Latitude': -24.00174855414096,
    'Longitude': -46.4127438597156
  },
  {
    'Latitude': -24.00336023443131,
    'Longitude': -46.41298988796774
  },
  {
    'Latitude': -24.00487711518463,
    'Longitude': -46.41325504309292
  },
  {
    'Latitude': -24.00580255585327,
    'Longitude': -46.41343980754293
  },
  {
    'Latitude': -24.00634891227965,
    'Longitude': -46.41352589527172
  },
  {
    'Latitude': -24.00650604956807,
    'Longitude': -46.41355694711312
  },
  {
    'Latitude': -24.00750551347667,
    'Longitude': -46.41361478961282
  },
  {
    'Latitude': -24.00768398584721,
    'Longitude': -46.41362222649547
  },
  {
    'Latitude': -24.007827262254,
    'Longitude': -46.41362956712959
  },
  {
    'Latitude': -24.00938773334738,
    'Longitude': -46.41402377327784
  },
  {
    'Latitude': -24.00941698880471,
    'Longitude': -46.41404717923913
  },
  {
    'Latitude': -24.00942140427438,
    'Longitude': -46.41411736686724
  },
  {
    'Latitude': -24.00940500500579,
    'Longitude': -46.41426666716923
  },
  {
    'Latitude': -24.00939598449335,
    'Longitude': -46.41442445660447
  },
  {
    'Latitude': -24.00940323920084,
    'Longitude': -46.41451645414318
  },
  {
    'Latitude': -24.00943614203454,
    'Longitude': -46.4146404582019
  },
  {
    'Latitude': -24.00955042276165,
    'Longitude': -46.41494104814704
  },
  {
    'Latitude': -24.0096635564843,
    'Longitude': -46.41523756180499
  },
  {
    'Latitude': -24.00979393419932,
    'Longitude': -46.41558671426203
  },
  {
    'Latitude': -24.01024947027495,
    'Longitude': -46.41677404787583
  },
  {
    'Latitude': -24.01037358572244,
    'Longitude': -46.41711780116831
  },
  {
    'Latitude': -24.01041737364121,
    'Longitude': -46.41723869063619
  },
  {
    'Latitude': -24.01043840589221,
    'Longitude': -46.417323134376
  },
  {
    'Latitude': -24.01046638621513,
    'Longitude': -46.41740525479515
  },
  {
    'Latitude': -24.01048552518195,
    'Longitude': -46.41750967550347
  },
  {
    'Latitude': -24.01047438074128,
    'Longitude': -46.4175965743146
  },
  {
    'Latitude': -24.01044030694379,
    'Longitude': -46.41769116853654
  },
  {
    'Latitude': -24.01041837177066,
    'Longitude': -46.41778211748758
  },
  {
    'Latitude': -24.01041041860577,
    'Longitude': -46.41788436560152
  },
  {
    'Latitude': -24.01040152598555,
    'Longitude': -46.41797919169873
  },
  {
    'Latitude': -24.0104293212401,
    'Longitude': -46.41806120318768
  },
  {
    'Latitude': -24.01046234311318,
    'Longitude': -46.41812506832754
  },
  {
    'Latitude': -24.01054535659656,
    'Longitude': -46.41821376265627
  },
  {
    'Latitude': -24.01061437012146,
    'Longitude': -46.41826078344787
  },
  {
    'Latitude': -24.01070048916512,
    'Longitude': -46.41829876538113
  },
  {
    'Latitude': -24.01080966082522,
    'Longitude': -46.4183899406633
  },
  {
    'Latitude': -24.01087639440681,
    'Longitude': -46.41845644694111
  },
  {
    'Latitude': -24.01093056999705,
    'Longitude': -46.41853336942388
  },
  {
    'Latitude': -24.01104259523302,
    'Longitude': -46.41886242549941
  },
  {
    'Latitude': -24.0112674444216,
    'Longitude': -46.41942284915048
  },
  {
    'Latitude': -24.01154517937976,
    'Longitude': -46.42016540634773
  },
  {
    'Latitude': -24.0116896800451,
    'Longitude': -46.42053843003107
  },
  {
    'Latitude': -24.01171369026787,
    'Longitude': -46.42060266918969
  },
  {
    'Latitude': -24.01172833008397,
    'Longitude': -46.42065584645678
  },
  {
    'Latitude': -24.01170989352709,
    'Longitude': -46.42068970824168
  },
  {
    'Latitude': -24.01164820638862,
    'Longitude': -46.420713230434
  },
  {
    'Latitude': -24.01086005501206,
    'Longitude': -46.42106520594777
  },
  {
    'Latitude': -24.01064778308993,
    'Longitude': -46.42115830477653
  },
  {
    'Latitude': -24.0106059370749,
    'Longitude': -46.4211445258938
  },
  {
    'Latitude': -24.01057408264756,
    'Longitude': -46.42113020622644
  },
  {
    'Latitude': -24.01052975586357,
    'Longitude': -46.42112560295055
  },
  {
    'Latitude': -24.01047537886066,
    'Longitude': -46.42113009257047
  },
  {
    'Latitude': -24.01043809963573,
    'Longitude': -46.42115080029479
  },
  {
    'Latitude': -24.01039125496807,
    'Longitude': -46.42118273050009
  },
  {
    'Latitude': -24.01037085181321,
    'Longitude': -46.42120921834799
  },
  {
    'Latitude': -24.01034268410211,
    'Longitude': -46.42123526167661
  },
  {
    'Latitude': -24.01031554326445,
    'Longitude': -46.42128312904556
  },
  {
    'Latitude': -24.01029410223291,
    'Longitude': -46.42131513003205
  },
  {
    'Latitude': -24.00980482605501,
    'Longitude': -46.42153703170179
  },
  {
    'Latitude': -24.00934801365241,
    'Longitude': -46.42174758022254
  },
  {
    'Latitude': -24.00888554551843,
    'Longitude': -46.42195705491974
  },
  {
    'Latitude': -24.00831937549694,
    'Longitude': -46.42221756364312
  },
  {
    'Latitude': -24.00793447141981,
    'Longitude': -46.42239541474091
  },
  {
    'Latitude': -24.00787279638264,
    'Longitude': -46.42241864795211
  },
  {
    'Latitude': -24.00783974104857,
    'Longitude': -46.42242684768488
  },
  {
    'Latitude': -24.00779776007903,
    'Longitude': -46.4224213308627
  },
  {
    'Latitude': -24.00775510893718,
    'Longitude': -46.42241332530197
  },
  {
    'Latitude': -24.00770484466987,
    'Longitude': -46.42240511229699
  },
  {
    'Latitude': -24.00765053864875,
    'Longitude': -46.42238215062465
  },
  {
    'Latitude': -24.0075977188038,
    'Longitude': -46.42233883263164
  },
  {
    'Latitude': -24.00756897122879,
    'Longitude': -46.42229889854178
  },
  {
    'Latitude': -24.00752101584493,
    'Longitude': -46.42222652467526
  },
  {
    'Latitude': -24.00746143092982,
    'Longitude': -46.42208584486448
  },
  {
    'Latitude': -24.00732426518254,
    'Longitude': -46.42172019350922
  },
  {
    'Latitude': -24.0072911530488,
    'Longitude': -46.42170090856684
  },
  {
    'Latitude': -24.0072585526807,
    'Longitude': -46.4216924913743
  },
  {
    'Latitude': -24.00721583159532,
    'Longitude': -46.42169297799898
  },
  {
    'Latitude': -24.00719469333901,
    'Longitude': -46.42171660424527
  },
  {
    'Latitude': -24.00716909909934,
    'Longitude': -46.42173342918476
  },
  {
    'Latitude': -24.00713186329716,
    'Longitude': -46.42172655522542
  },
  {
    'Latitude': -24.00545740181174,
    'Longitude': -46.4209707101109
  },
  {
    'Latitude': -24.00541466134813,
    'Longitude': -46.42094368253283
  },
  {
    'Latitude': -24.00538242451978,
    'Longitude': -46.42091857693585
  },
  {
    'Latitude': -24.00535312882958,
    'Longitude': -46.42084243666827
  },
  {
    'Latitude': -24.00490128137871,
    'Longitude': -46.41964277800133
  },
  {
    'Latitude': -24.00489291443165,
    'Longitude': -46.41960416784004
  },
  {
    'Latitude': -24.00493462395513,
    'Longitude': -46.41957092985287
  },
  {
    'Latitude': -24.00523124660531,
    'Longitude': -46.41949655441111
  },
  {
    'Latitude': -24.00572375902496,
    'Longitude': -46.41937325936479
  },
  {
    'Latitude': -24.00575763884437,
    'Longitude': -46.419359168909
  },
  {
    'Latitude': -24.00577556949241,
    'Longitude': -46.41934212584945
  },
  {
    'Latitude': -24.00580241693091,
    'Longitude': -46.41930280130935
  },
  {
    'Latitude': -24.00580850067206,
    'Longitude': -46.41927044008876
  },
  {
    'Latitude': -24.00592137993822,
    'Longitude': -46.41836608545211
  },
  {
    'Latitude': -24.00592266018606,
    'Longitude': -46.4183436934664
  },
  {
    'Latitude': -24.00590495332802,
    'Longitude': -46.41831669144998
  },
  {
    'Latitude': -24.00587100705116,
    'Longitude': -46.41829185983674
  },
  {
    'Latitude': -24.00578764547779,
    'Longitude': -46.41828994543177
  },
  {
    'Latitude': -24.0051107930196,
    'Longitude': -46.41817423471825
  },
  {
    'Latitude': -24.00474893391343,
    'Longitude': -46.41811576818115
  },
  {
    'Latitude': -24.0038246543832,
    'Longitude': -46.41796108856455
  },
  {
    'Latitude': -24.00319108765279,
    'Longitude': -46.41786868237875
  },
  {
    'Latitude': -24.00305220129489,
    'Longitude': -46.41785768463722
  },
  {
    'Latitude': -24.00302974444641,
    'Longitude': -46.41786276189231
  },
  {
    'Latitude': -24.00300465643703,
    'Longitude': -46.41788279069896
  },
  {
    'Latitude': -24.00299068515081,
    'Longitude': -46.41791098086444
  },
  {
    'Latitude': -24.00298954515449,
    'Longitude': -46.41795382077843
  },
  {
    'Latitude': -24.00324474918456,
    'Longitude': -46.41867167434914
  },
  {
    'Latitude': -24.00359811593672,
    'Longitude': -46.41959181007281
  },
  {
    'Latitude': -24.00374849047693,
    'Longitude': -46.42001589800783
  },
  {
    'Latitude': -24.0038160306371,
    'Longitude': -46.42021394819473
  },
  {
    'Latitude': -24.00404041569036,
    'Longitude': -46.42081372860618
  },
  {
    'Latitude': -24.00445716951314,
    'Longitude': -46.42194070625489
  },
  {
    'Latitude': -24.00478404735044,
    'Longitude': -46.42283155554802
  },
  {
    'Latitude': -24.00505908985251,
    'Longitude': -46.42356158030873
  },
  {
    'Latitude': -24.00507638213608,
    'Longitude': -46.42359441933316
  },
  {
    'Latitude': -24.00508740269129,
    'Longitude': -46.42363385163405
  },
  {
    'Latitude': -24.00509364368221,
    'Longitude': -46.42366354363936
  },
  {
    'Latitude': -24.00509406801959,
    'Longitude': -46.42370882425367
  },
  {
    'Latitude': -24.0050523219373,
    'Longitude': -46.42373438242803
  },
  {
    'Latitude': -24.00496825527007,
    'Longitude': -46.42377195346003
  },
  {
    'Latitude': -24.00482837379242,
    'Longitude': -46.42383359632866
  },
  {
    'Latitude': -24.00391960587092,
    'Longitude': -46.4242403506036
  },
  {
    'Latitude': -24.00339851076487,
    'Longitude': -46.42446996982105
  },
  {
    'Latitude': -24.00337260002064,
    'Longitude': -46.42450722748077
  },
  {
    'Latitude': -24.00336612484193,
    'Longitude': -46.42453763183676
  },
  {
    'Latitude': -24.00339796299932,
    'Longitude': -46.42463654888891
  },
  {
    'Latitude': -24.00362448005808,
    'Longitude': -46.42527945264489
  },
  {
    'Latitude': -24.00382303359188,
    'Longitude': -46.42580116394345
  },
  {
    'Latitude': -24.00527262932875,
    'Longitude': -46.4297268804315
  },
  {
    'Latitude': -24.00568283549402,
    'Longitude': -46.43082740211872
  },
  {
    'Latitude': -24.00622007431507,
    'Longitude': -46.43225530614839
  },
  {
    'Latitude': -24.00638800990377,
    'Longitude': -46.43272448406457
  },
  {
    'Latitude': -24.00660500282982,
    'Longitude': -46.43330394993173
  },
  {
    'Latitude': -24.00677696835281,
    'Longitude': -46.43378871190116
  },
  {
    'Latitude': -24.0070735704846,
    'Longitude': -46.43458034850719
  },
  {
    'Latitude': -24.00743536620728,
    'Longitude': -46.43555205184706
  },
  {
    'Latitude': -24.00795862613388,
    'Longitude': -46.43695554105101
  },
  {
    'Latitude': -24.008368645069,
    'Longitude': -46.43805555132509
  },
  {
    'Latitude': -24.008561085502,
    'Longitude': -46.43860316263751
  },
  {
    'Latitude': -24.00989854843403,
    'Longitude': -46.4421655477527
  },
  {
    'Latitude': -24.01054207498818,
    'Longitude': -46.44399275027093
  },
  {
    'Latitude': -24.01126362127694,
    'Longitude': -46.44604753260047
  },
  {
    'Latitude': -24.01140025397426,
    'Longitude': -46.44649974221407
  },
  {
    'Latitude': -24.01163020725016,
    'Longitude': -46.44724898611499
  },
  {
    'Latitude': -24.0118758855583,
    'Longitude': -46.4480735105912
  },
  {
    'Latitude': -24.01212642631659,
    'Longitude': -46.44900977872091
  },
  {
    'Latitude': -24.01227775560711,
    'Longitude': -46.44968455242582
  },
  {
    'Latitude': -24.01251305318357,
    'Longitude': -46.45060407850554
  },
  {
    'Latitude': -24.01277788893563,
    'Longitude': -46.4516783826643
  },
  {
    'Latitude': -24.01357574863062,
    'Longitude': -46.45504226031687
  },
  {
    'Latitude': -24.0142862571579,
    'Longitude': -46.45801308260815
  },
  {
    'Latitude': -24.01492379417958,
    'Longitude': -46.46071909544347
  },
  {
    'Latitude': -24.01524993992456,
    'Longitude': -46.46196017592065
  },
  {
    'Latitude': -24.01562777376353,
    'Longitude': -46.4633945233808
  },
  {
    'Latitude': -24.01575401277445,
    'Longitude': -46.46389613868507
  },
  {
    'Latitude': -24.01582039375274,
    'Longitude': -46.4641339373692
  },
  {
    'Latitude': -24.01593268871612,
    'Longitude': -46.46458616365234
  },
  {
    'Latitude': -24.01611427236869,
    'Longitude': -46.4652662190826
  },
  {
    'Latitude': -24.01620771782461,
    'Longitude': -46.46563116185844
  },
  {
    'Latitude': -24.01643044469963,
    'Longitude': -46.46634252838087
  },
  {
    'Latitude': -24.01668945224681,
    'Longitude': -46.46717276811743
  },
  {
    'Latitude': -24.01689526191213,
    'Longitude': -46.46780203918812
  },
  {
    'Latitude': -24.01719838768414,
    'Longitude': -46.46878142972854
  },
  {
    'Latitude': -24.01795320405145,
    'Longitude': -46.47112852495398
  },
  {
    'Latitude': -24.01823805598142,
    'Longitude': -46.4720113731135
  },
  {
    'Latitude': -24.01842881556634,
    'Longitude': -46.47260776650114
  },
  {
    'Latitude': -24.01883487777107,
    'Longitude': -46.47387981522673
  },
  {
    'Latitude': -24.01929446082977,
    'Longitude': -46.47532316254062
  },
  {
    'Latitude': -24.01958565687652,
    'Longitude': -46.4762165701134
  },
  {
    'Latitude': -24.01991209516057,
    'Longitude': -46.47720169094012
  },
  {
    'Latitude': -24.02033081434668,
    'Longitude': -46.47852804881603
  },
  {
    'Latitude': -24.02075774961821,
    'Longitude': -46.47987155743708
  },
  {
    'Latitude': -24.02112803009742,
    'Longitude': -46.48101540251878
  },
  {
    'Latitude': -24.02125123291735,
    'Longitude': -46.48138680570205
  },
  {
    'Latitude': -24.02221884380523,
    'Longitude': -46.48437089673791
  },
  {
    'Latitude': -24.0234396091015,
    'Longitude': -46.48818681329503
  },
  {
    'Latitude': -24.02435503612739,
    'Longitude': -46.49099617936219
  },
  {
    'Latitude': -24.02546512864777,
    'Longitude': -46.49451421931212
  },
  {
    'Latitude': -24.0257564416275,
    'Longitude': -46.49536162458288
  },
  {
    'Latitude': -24.02593332211868,
    'Longitude': -46.49587786815377
  },
  {
    'Latitude': -24.02603361002336,
    'Longitude': -46.49615846513802
  },
  {
    'Latitude': -24.02627637686543,
    'Longitude': -46.49685143351212
  },
  {
    'Latitude': -24.02633207988829,
    'Longitude': -46.49699265805711
  },
  {
    'Latitude': -24.02652955810233,
    'Longitude': -46.49749454275756
  },
  {
    'Latitude': -24.02667792363638,
    'Longitude': -46.497825839222
  },
  {
    'Latitude': -24.02671078638969,
    'Longitude': -46.49790786826284
  },
  {
    'Latitude': -24.02673896818719,
    'Longitude': -46.49794975097458
  },
  {
    'Latitude': -24.02677736050227,
    'Longitude': -46.49797910554114
  },
  {
    'Latitude': -24.02682291422663,
    'Longitude': -46.49797570398162
  },
  {
    'Latitude': -24.02690397683781,
    'Longitude': -46.49789129080732
  },
  {
    'Latitude': -24.02694141322433,
    'Longitude': -46.49784599157735
  },
  {
    'Latitude': -24.02735492832857,
    'Longitude': -46.49732166506126
  },
  {
    'Latitude': -24.02851618247911,
    'Longitude': -46.49571804291602
  },
  {
    'Latitude': -24.02861017491827,
    'Longitude': -46.49562897439657
  },
  {
    'Latitude': -24.02868872142052,
    'Longitude': -46.49562907709979
  },
  {
    'Latitude': -24.02874743223266,
    'Longitude': -46.49570830160327
  },
  {
    'Latitude': -24.02878757824783,
    'Longitude': -46.49579733979693
  },
  {
    'Latitude': -24.02887289937793,
    'Longitude': -46.49601743302431
  }
]";
        }



        static double Haversine(Position from, Position to)
        {
            const double EarthRadius = 3958.756;

            double difference_lat = DegreesToRadians(to.Latitude - from.Latitude);
            double difference_lon = DegreesToRadians(to.Longitude - from.Longitude);

            double alpha = Math.Sin(difference_lat / 2) * Math.Sin(difference_lat / 2) +
                                Math.Cos(DegreesToRadians(from.Latitude)) *
                                Math.Cos(DegreesToRadians(to.Latitude)) *
                                Math.Sin(difference_lon / 2) * Math.Sin(difference_lon / 2);
            var ret = 2 * Math.Atan2(Math.Sqrt(alpha), Math.Sqrt(1 - alpha)) * EarthRadius;

            Debug.WriteLine(ret);

            return ret;
        }

        /// <summary>
        /// Função para conversão de graus em radianos
        /// </summary>
        /// <param name="degrees">Graus</param>
        /// <returns></returns>
        static double DegreesToRadians(double degrees)
        {
            var ret = degrees / 180 * Math.PI;
            return ret;
        }
    }
}
