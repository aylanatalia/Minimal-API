using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using minimal_api.Dominio.Entidades;
using minimal_api.Infraestrutura.Db;
using minimal_api.Dominio.Servicos;

namespace Test.Domain.Servicos
{
    [TestClass]
    public class AdministradorServicoTest
    {
        private DbContexto CriarContextoDeTeste()
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var path = Path.GetFullPath(Path.Combine(assemblyPath ?? "", "..", "..", ".."));

            var builder = new ConfigurationBuilder().SetBasePath(path ?? Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).AddEnvironmentVariables();

            var configuration = builder.Build();
            return new DbContexto(configuration);
        }

        [TestMethod]
        public void TestandoSalvarAdministrador()
        {
            var contexto = CriarContextoDeTeste();
            contexto.Database.ExecuteSqlRaw("TRUNCATE TABLE Administradores");

            var adm = new Administrador();
            adm.Id = 1;
            adm.Email = "teste@teste.com";
            adm.Senha = "123456";
            adm.Perfil = "Adm";

            var administradorServico = new AdministradorServico(contexto);

            administradorServico.Incluir(adm);

            Assert.AreEqual(1, administradorServico.Todos(1).Count);
        }

        [TestMethod]
        public void TestandoBuscaPorId()
        {
            // Arrange
            var context = CriarContextoDeTeste();
            context.Database.ExecuteSqlRaw("TRUNCATE TABLE Administradores");

            var adm = new Administrador();
            adm.Email = "teste@teste.com";
            adm.Senha = "teste";
            adm.Perfil = "Adm";

            var administradorServico = new AdministradorServico(context);

            // Act
            administradorServico.Incluir(adm);
            var admDoBanco = administradorServico.BuscaPorId(adm.Id);

            // Assert
            Assert.AreEqual(1, admDoBanco?.Id);
        }
    }
}