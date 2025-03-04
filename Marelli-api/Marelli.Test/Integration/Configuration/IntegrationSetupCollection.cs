using Xunit;

namespace Marelli.Test.Integration.Configuration
{
    [CollectionDefinition("Integration collection")]
    public class IntegrationSetupCollection : ICollectionFixture<IntegrationSetupFixture>
    {
    }
}
