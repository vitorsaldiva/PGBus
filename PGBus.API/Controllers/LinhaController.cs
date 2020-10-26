using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PGBus.API.Model;

namespace PGBus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LinhaController : ControllerBase, ILinhaController
    {
        private readonly IPiracicabanaService service;

        public LinhaController(IPiracicabanaService service)
        {
            this.service = service;
        }

        [HttpGet("{linhaId}")]
        public async Task<ActionResult<Linha>> ObterLinhaPorId(string linhaId)
        {
            var linha = await service.ObterLinhaPorId(linhaId);

            if (linha == null)
                return NotFound();

            return linha;
        }
    }
}