using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using System.Net;
using PGBus.API.Model;

namespace PGBus.API.IntegrationTests
{
    public class UnitTest1
    {
        private readonly HttpClient client;
        private readonly string baseUrl;
        private readonly string endpoint;

        public UnitTest1()
        {
            baseUrl = "https://localhost:44390";
            endpoint = "linha";
            var appFactory = new WebApplicationFactory<Startup>();
            client = appFactory.CreateClient();
        }

        [Fact]
        public async Task Test1()
        {
            //Arrange
            var id = "1234";

            //Act
            var response = await client.GetAsync($"{baseUrl}/{endpoint}/{id}");
            var linha = await response.Content.ReadAsAsync<Linha>();

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            linha.Should().NotBeNull();
        }
    }
}
