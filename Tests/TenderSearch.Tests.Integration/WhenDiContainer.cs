using Eml.DataRepository.Attributes;
using Eml.DataRepository.Extensions;
using Shouldly;
using TenderSearch.Tests.Integration.BaseClasses;
using Xunit;

namespace TenderSearch.Tests.Integration
{
    public class WhenDiContainer : IntegrationTestDiBase
    {
        [Fact]
        public void ProductionMigrator_ShouldBeDiscoverable()
        {
            var dbMigration = classFactory.GetMigrator(Environments.PRODUCTION);

            dbMigration.ShouldNotBeNull();
        }
    }
}
