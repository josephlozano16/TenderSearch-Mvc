using System.Web.Mvc;
using TenderSearch.Contracts.Infrastructure;
using TenderSearch.Web.Controllers.BaseClasses;

namespace TenderSearch.Web.Areas.Users.Controllers
{
    [Authorize(Roles = Authorize.Users)]
    public class HomeController : HomeControllerBase
    {
    }
}