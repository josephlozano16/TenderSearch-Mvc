using Shouldly;
using TenderSearch.Tests.Integration.BaseClasses;
using Xunit;

namespace TenderSearch.Tests.Integration
{
    public class WhenIdentityMigrations : IntegrationTestDbBase
    {
        [Fact]
        private void Migration_ShouldExecute()
        {
            ClassFactory.ShouldNotBeNull();
        }
    }
}
