using Microsoft.AspNetCore.Mvc;
using PGBus.API.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PGBus.API
{
    public interface ILinhaController
    {
        Task<ActionResult<Linha>> ObterLinhaPorId(string linhaId);
    }
}