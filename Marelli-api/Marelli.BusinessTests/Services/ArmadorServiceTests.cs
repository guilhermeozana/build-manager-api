using Microsoft.VisualStudio.TestTools.UnitTesting;
using Demurrage.Business.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Demurrage.Domain.Entities;

namespace Demurrage.Business.Services.Tests
{
    [TestClass()]
    public class ArmadorServiceTests
    {
        private ArmadorService? _armadorService;

        [TestMethod()]
        public async Task CreateAsyncTest()
        {
            if(_armadorService == null)
                Assert.Fail("Dependency injection error");

            var armador = new Armador()
            {
                Id = 0,
                Apelido = "ApelidoSomenteStringText",
                Nome = "NomeSomenteStringText",
                Ativo = true,
                Codigo = "CodigoArmador123",
                SCACCode = "ScacCode123"
            };

            var rowsReturned = await _armadorService.CreateAsync(armador);

            Assert.AreEqual(1, rowsReturned);

            Assert.IsTrue(rowsReturned == 1, "Retorno de linhas afetadas deveria ser igual a 1.");
        }
    }
}