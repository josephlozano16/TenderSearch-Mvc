using Eml.ClassFactory.Contracts;
using Eml.Mediator.Contracts;
using Xunit;

namespace TenderSearch.Tests.Integration.BaseClasses
{
    [Collection(IntegrationTestDbFixture.COLLECTION_DEFINITION)]
    public abstract class IntegrationTestDbBase
    {
        protected readonly IClassFactory ClassFactory;

        protected readonly IMediator Mediator;

        protected IntegrationTestDbBase()
        {
            ClassFactory = IntegrationTestDbFixture.ClassFactory;

            Mediator = ClassFactory.GetExport<IMediator>();
        }
    }
}
