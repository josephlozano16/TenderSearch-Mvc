using System.Web.Mvc;
using ControllerBase = Eml.ControllerBase.Mvc.ControllerBase;

namespace TenderSearch.Web.Controllers
{
    public class HomeController : ControllerBase
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Register()
        {
            return RedirectToAction<AccountController>(c => c.Register());
        }

        public ActionResult ForgotPassword()
        {
            return RedirectToAction<AccountController>(c => c.ForgotPassword());
        }
    }
}