using System;
using System.Collections.Generic;
using System.Text;

namespace PGBus.Models
{
    class Veiculo
    {
        public string Prefixo { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public string Sentido { get; set; }
        public string Conteudo { get; set; }
    }
}
