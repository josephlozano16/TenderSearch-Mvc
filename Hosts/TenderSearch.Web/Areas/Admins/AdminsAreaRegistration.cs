using System.Web.Mvc;

namespace TenderSearch.Web.Areas.Admins
{
    public class AdminsAreaRegistration : AreaRegistration 
    {
        public override string AreaName => "Admins";

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Admins_default",
                "Admins/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "TenderSearch.Web.Areas.Admins.Controllers" }
            );
        }
    }
}