using PGBus.API.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PGBus.API
{
    public interface IPiracicabanaService
    {
        Task<Linha> ObterLinhaPorId(string linhaId);
    }
}