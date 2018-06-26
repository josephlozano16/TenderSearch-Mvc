using System.Web.Mvc;
using TenderSearch.Contracts.Infrastructure;
using TenderSearch.Web.Controllers.BaseClasses;

namespace TenderSearch.Web.Areas.Admins.Controllers
{
    [Authorize(Roles = Authorize.Admins)]
    public class HomeController :   HomeControllerBase
    {
    }
}