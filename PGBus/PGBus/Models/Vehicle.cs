using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PGBus.Models
{
    public class Vehicle : ObservableObject
    {
        public string Prefixo { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public string Sentido { get; set; }
        public string Conteudo { get; set; }
    }
}
