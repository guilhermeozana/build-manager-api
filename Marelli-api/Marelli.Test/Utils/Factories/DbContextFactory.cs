using Marelli.Infra.Context;
using Microsoft.EntityFrameworkCore;

namespace Marelli.Test.Utils.Factories
{
    public class DbContextFactory
    {
        public static DemurrageContext GetDemurrageContextTest()
        {
            return new DemurrageContext(GetInMemoryDbOptions());
        }

        public static DbContextOptions<DemurrageContext> GetInMemoryDbOptions()
        {
            return new DbContextOptionsBuilder<DemurrageContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }
    }
}
