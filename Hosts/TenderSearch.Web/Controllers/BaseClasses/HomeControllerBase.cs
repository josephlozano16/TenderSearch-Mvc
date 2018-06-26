using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using TenderSearch.Web.IdentityConfig;
using ControllerBase = Eml.ControllerBase.Mvc.ControllerBase;

namespace TenderSearch.Web.Controllers.BaseClasses
{
    public abstract class HomeControllerBase : ControllerBase
    {
        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get => _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            private set => _userManager = value;
        }
        public virtual async Task<ActionResult> Index()
        {
            var user = await UserManager.FindByNameAsync(User.Identity.Name);
            var rolesForUser = await UserManager.GetRolesAsync(user.Id);
            ViewBag.RoleCount = rolesForUser.Count;
            return View();
        }

        protected void SetUserManager(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }
    }
}