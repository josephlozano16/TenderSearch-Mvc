using Eml.ClassFactory.Contracts;
using Xunit;

namespace TenderSearch.Tests.Integration.BaseClasses
{
    [Collection(IntegrationTestDiFixture.COLLECTION_DEFINITION)]
    public abstract class IntegrationTestDiBase
    {
        protected readonly IClassFactory classFactory;

        public IntegrationTestDiBase()
        {
            classFactory = IntegrationTestDiFixture.ClassFactory;
        }
    }
}
