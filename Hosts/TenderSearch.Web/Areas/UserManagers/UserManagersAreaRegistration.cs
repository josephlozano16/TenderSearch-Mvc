using System.Web.Mvc;

namespace TenderSearch.Web.Areas.UserManagers
{
    public class UserManagersAreaRegistration : AreaRegistration 
    {
        public override string AreaName => "UserManagers";

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "UserManagers_default",
                "UserManagers/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "TenderSearch.Web.Areas.UserManagers.Controllers" }
            );
        }
    }
}