using System.Threading.Tasks;
using System.Web.Mvc;
using TenderSearch.Contracts.Infrastructure;
using TenderSearch.Web.Controllers.BaseClasses;

namespace TenderSearch.Web.Areas.UserManagers.Controllers
{
    [Authorize(Roles = Authorize.UserManagers)]
    public class HomeController : HomeControllerBase
    {
        public override async Task<ActionResult> Index()
        {
            var tmpIndex = await base.Index();
            return RedirectToAction<AspNetUserController>(c => c.Index(string.Empty, null, 1, null), "UserManagers");
        }
    }
}