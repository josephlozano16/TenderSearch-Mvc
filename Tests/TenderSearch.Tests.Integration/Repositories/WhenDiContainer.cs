using Eml.DataRepository.Contracts;
using Shouldly;
using TenderSearch.Business.Common.Entities;
using TenderSearch.Tests.Integration.BaseClasses;
using Xunit;

namespace TenderSearch.Tests.Integration.Repositories
{
    public class WhenDiContainer : IntegrationTestDbBase
    {
        [Fact]
        public void CompanyRepository_ShouldBeDiscoverable()
        {
            var sut = ClassFactory.GetExport<IDataRepositorySoftDeleteInt<Company>>();

            sut.ShouldNotBeNull();
        }

        [Fact]
        public void ContactPersonRepository_ShouldBeDiscoverable()
        {
            var sut = ClassFactory.GetExport<IDataRepositorySoftDeleteInt<ContactPerson>>();

            sut.ShouldNotBeNull();
        }

        [Fact]
        public void ContractRepository_ShouldBeDiscoverable()
        {
            var sut = ClassFactory.GetExport<IDataRepositorySoftDeleteInt<Contract>>();

            sut.ShouldNotBeNull();
        }

        [Fact]
        public void PositionTitleRepository_ShouldBeDiscoverable()
        {
            var sut = ClassFactory.GetExport<IDataRepositorySoftDeleteInt<PositionTitle>>();

            sut.ShouldNotBeNull();
        }
    }
}