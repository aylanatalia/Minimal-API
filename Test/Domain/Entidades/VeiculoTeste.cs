using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using minimal_api.Dominio.Entidades;

namespace Test.Domain.Entidades
{
    [TestClass]
    public class VeiculoTeste
    {
        [TestMethod]
        public void TestarPropriedadesVeiculo()
        {
            var veiculo = new Veiculo();

            veiculo.Id = 1;
            veiculo.Nome = "Ford";
            veiculo.Marca = "Fiesta";
            veiculo.Ano = 2023;

            Assert.AreEqual(1, veiculo.Id);
            Assert.AreEqual("Ford", veiculo.Nome);
            Assert.AreEqual("Fiesta", veiculo.Marca);
            Assert.AreEqual(2023, veiculo.Ano);
        }
    }
}