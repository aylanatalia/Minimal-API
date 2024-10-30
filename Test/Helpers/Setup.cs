using System;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using minimal_api;
using minimal_api.Dominio.Interfaces;
using Test.Mocks;

namespace Test.Helpers
{
    public class Setup
    {
        public const string PORT = "5001";
        public static TestContext testContext = default!;
        private static WebApplicationFactory<Startup> http = default!;
        public static HttpClient client = default!;

        public static void ClassInit(TestContext testContext)
        {
            Setup.testContext = testContext;
            Setup.http = new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseSetting("https_port", Setup.PORT).UseEnvironment("Testing");
                    builder.ConfigureServices(services =>
                    {
                        services.AddScoped<iAdministradorServicos, AdministradorServicoMock>();
                    });
                });

            Setup.client = Setup.http.CreateClient();
        }

        public static void ClassCleanup()
        {
            Setup.http.Dispose();
        }
    }
}
