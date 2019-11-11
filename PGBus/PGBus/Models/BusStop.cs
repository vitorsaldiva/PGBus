﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

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
    }
}