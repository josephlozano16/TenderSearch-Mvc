using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using TenderSearch.Contracts.Infrastructure;
using TenderSearch.Web.IdentityConfig;

namespace TenderSearch.Web.Controllers
{
    [Authorize]
    public class DashboardController : Eml.ControllerBase.Mvc.ControllerBase
    {
        public async Task<ActionResult> Index()
        {
            var user = await UserManager.FindByNameAsync(User.Identity.Name);
            var rolesForUser = await UserManager.GetRolesAsync(user.Id);

            //no need to  display dashboard if the user have 1 role. applicable only to users with multiple roles
            if (rolesForUser.Count == 1)
            {
                return Goto(rolesForUser.First());
            }
            var model = rolesForUser.ToList();

            return View(model.OrderBy(x => x));
        }

        public ActionResult Goto(string jump)
        {
            var eRole = (UserRoles)Enum.Parse(typeof(UserRoles), jump);
            switch (eRole)
            {
                case UserRoles.UserManagers:
                    return RedirectToAction<Areas.UserManagers.Controllers.HomeController>(c => c.Index(), jump);

                case UserRoles.Users:
                    return RedirectToAction<Areas.Users.Controllers.HomeController>(c => c.Index(), jump);

                case UserRoles.Admins:
                    return RedirectToAction<Areas.Admins.Controllers.HomeController>(c => c.Index(), jump);

                default:
                    return RedirectToAction<AccountController>(c => c.Register());
            }
        }

        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get => _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            private set => _userManager = value;
        }
    }
}