using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using PGBus.API.Controllers;
using PGBus.API.Model;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace PGBus.API.Tests
{
    public class ApiTests
    {
        ILinhaController linhaController;
        Mock<IPiracicabanaService> service;

        [SetUp]
        public void Setup()
        {
            service = new Mock<IPiracicabanaService>();
        }

        [Test]
        public async Task Api_ObterInformacoesLinhaPorId_DeveRetornarDtoComInfos()
        {
            //Arrange
            var idLinha = "11PR";
            service.Setup(s => s.ObterLinhaPorId(It.IsAny<string>())).Returns(() =>
            {
                var linha = new Linha { Codigo = Guid.NewGuid().ToString(), Descricao = "11PR - SOLEMAR", LinhaId = "11PR" };
                return Task.Run(() => linha);
            });
            linhaController = new LinhaController(service.Object);

            //Act
            var retorno = await linhaController.ObterLinhaPorId(idLinha);

            //Assert
            Assert.IsNotNull(retorno);
            Assert.IsNull(retorno.Result);
            Assert.AreEqual(retorno.Value.LinhaId, idLinha);
        }

        [Test]
        public async Task Api_ObterInformacoesLinhaPorId_DeveRetornarNull()
        {
            //Arrange
            var idLinha = "11PR";
            service.Setup(s => s.ObterLinhaPorId(It.IsAny<string>()));
            linhaController = new LinhaController(service.Object);

            //Act
            var retorno = await linhaController.ObterLinhaPorId(idLinha);

            //Assert
            Assert.IsNotNull(retorno);
            Assert.IsInstanceOf<NotFoundResult>(retorno.Result);
        }
    }
}