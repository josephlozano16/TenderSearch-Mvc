using Shouldly;
using TenderSearch.Tests.Integration.BaseClasses;
using TenderSearch.Web.Areas.UserManagers.Controllers;
using TenderSearch.Web.Areas.Users.Controllers;
using Xunit;

namespace TenderSearch.Tests.Integration.Controllers
{
    public class WhenDiContainer : IntegrationTestDbBase
    {
        [Fact]
        public void CompanyController_ShouldBeDiscoverable()
        {
            var sut = ClassFactory.GetExport<CompanyController>();

            sut.ShouldNotBeNull();
        }

        [Fact]
        public void ContactPersonController_ShouldBeDiscoverable()
        {
            var sut = ClassFactory.GetExport<ContactPersonController>();

            sut.ShouldNotBeNull();
        }

        [Fact]
        public void ContractController_ShouldBeDiscoverable()
        {
            var sut = ClassFactory.GetExport<ContractController>();

            sut.ShouldNotBeNull();
        }

        [Fact]
        public void PositionTitleController_ShouldBeDiscoverable()
        {
            var sut = ClassFactory.GetExport<PositionTitleController>();

            sut.ShouldNotBeNull();
        }

        [Fact]
        public void AspNetUserRoleController_ShouldBeDiscoverable()
        {
            var sut = ClassFactory.GetExport<AspNetUserRoleController>();

            sut.ShouldNotBeNull();
        }

        [Fact]
        public void AspNetUserController_ShouldBeDiscoverable()
        {
            var sut = ClassFactory.GetExport<AspNetUserController>();

            sut.ShouldNotBeNull();
        }
    }
}