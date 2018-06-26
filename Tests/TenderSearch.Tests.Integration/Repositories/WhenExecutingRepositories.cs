using Eml.DataRepository.Contracts;
using Shouldly;
using TenderSearch.Business.Common.Entities;
using TenderSearch.Tests.Integration.BaseClasses;
using Xunit;

namespace TenderSearch.Tests.Integration.Repositories
{
    public class WhenExecutingRepositories : IntegrationTestDbBase
    {
        [Fact]
        public void CompanyRepository_ShouldRetrieveData()
        {
            var repository = ClassFactory.GetExport<IDataRepositorySoftDeleteInt<Company>>();

            var items = repository.GetAll();

            items.Count.ShouldBe(2);
        }

        [Fact]
        public void ContactPersonRepository_ShouldRetrieveData()
        {
            var repository = ClassFactory.GetExport<IDataRepositorySoftDeleteInt<ContactPerson>>();

            var items = repository.GetAll();

            items.Count.ShouldBe(1);
        }

        [Fact]
        public void ContractRepository_ShouldRetrieveData()
        {
            var repository = ClassFactory.GetExport<IDataRepositorySoftDeleteInt<Contract>>();

            var items = repository.GetAll();

            items.Count.ShouldBe(2);
        }

        [Fact]
        public void PositionTitleRepository_ShouldRetrieveData()
        {
            var repository = ClassFactory.GetExport<IDataRepositorySoftDeleteInt<PositionTitle>>();

            var items = repository.GetAll();

            items.Count.ShouldBe(3);
        }
    }
}