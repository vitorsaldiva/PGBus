using PGBus.API.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PGBus.API.Service
{
    public class PiracicabanaService : IPiracicabanaService
    {
        public Task<Linha> ObterLinhaPorId(string linhaId)
        {
            return Task.FromResult(new Linha
            {
                Codigo = "11PR",
                Descricao = "Linha 11",
                DescricaoCompleta = "",
                LinhaId = Guid.NewGuid().ToString()
            });
        }
    }
}
