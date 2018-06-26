using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using TenderSearch.Contracts.Infrastructure;
using System.Net.Mail;
using NLog;
using TenderSearch.Data.Security;
using TenderSearch.Web.Configurations;
using TenderSearch.Web.IdentityConfig;
using TenderSearch.Web.Utils;
using LogLevel = NLog.LogLevel;
using ControllerBase = Eml.ControllerBase.Mvc.ControllerBase;
using ExternalLoginConfirmationViewModel = TenderSearch.Web.ViewModels.AccountViewModels.ExternalLoginConfirmationViewModel;
using ForgotPasswordViewModel = TenderSearch.Web.ViewModels.AccountViewModels.ForgotPasswordViewModel;
using LoginViewModel = TenderSearch.Web.ViewModels.AccountViewModels.LoginViewModel;
using RegisterViewModel = TenderSearch.Web.ViewModels.AccountViewModels.RegisterViewModel;
using ResetPasswordViewModel = TenderSearch.Web.ViewModels.AccountViewModels.ResetPasswordViewModel;
using SendCodeViewModel = TenderSearch.Web.ViewModels.AccountViewModels.SendCodeViewModel;
using VerifyCodeViewModel = TenderSearch.Web.ViewModels.AccountViewModels.VerifyCodeViewModel;

namespace TenderSearch.Web.Controllers
{
    [Authorize]
    public class AccountController : ControllerBase
    {
        private ApplicationSignInManager _signInManager;
        protected Logger Logger { get; private set; }

        public AccountController()
        {
            Logger = LogManager.GetCurrentClassLogger();
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get => _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            private set => _signInManager = value;
        }

        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get => _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            private set => _userManager = value;
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            var userName = GetProperCase(User.Identity.Name);
            var model = new LoginViewModel { RememberMe = false, UserName = userName };
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    var user = await UserManager.FindByNameAsync(model.UserName);
                    var rolesForUser = await UserManager.GetRolesAsync(user.Id);
                    if (rolesForUser.Count == 0)
                    {
                        ModelState.AddModelError("", "No roles assigned. Please contact your Administrator.");
                        return View(model);
                    }

                    if (!string.IsNullOrWhiteSpace(returnUrl)) return RedirectToLocal(returnUrl);

                    if (rolesForUser.Count > 1)
                    {
                        return RedirectToAction<DashboardController>(c => c.Index(), string.Empty);
                    }
                    foreach (var role in rolesForUser)
                    {

                        var eRole = (UserRoles)Enum.Parse(typeof(UserRoles), role);
                        switch (eRole)
                        {
                            case UserRoles.Users:
                            case UserRoles.Admins:
                            case UserRoles.UserManagers:
                                return RedirectToAction<HomeController>(c => c.Index(), role);
                            default:
                                return RedirectToAction<AccountController>(c => c.Register());
                        }
                    }

                    break;
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction<AccountController>(c => c.SendCode(returnUrl, model.RememberMe));
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
            return RedirectToLocal(returnUrl);
        }

        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (await SignInManager.HasBeenVerifiedAsync())
                return View(new VerifyCodeViewModel
                {
                    Provider = provider,
                    ReturnUrl = returnUrl,
                    RememberMe = rememberMe
                });
            ViewBag.ErrorMessage = "SignInManager.HasBeenVerifiedAsync has return false.";
            return PartialView("Error");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        [AllowAnonymous]
        public ActionResult Register()
        {
            var userName = GetProperCase(User.Identity.Name);
            var model = new RegisterViewModel { UserName = userName };
            return View(model);

        }

        private static string GetProperCase(string s)
        {
            var result = s;
            var aWords = new List<string>();
            aWords.AddRange(s.Split('.'));
            if (aWords.Count <= 1) return result;

            aWords = aWords.ConvertAll(r => r.Length > 1 ? $"{r.Substring(0, 1).ToUpper()}{r.Substring(1)}" : r.ToUpper());
            result = string.Join(".", aWords.ToArray());
            return result;
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userName = GetProperCase(model.UserName);
            var user = new ApplicationUser { UserName = userName, Email = model.Email };
            var result = await UserManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                //TODO Send email notification to UserManagers
                NotifyUserManagers(userName);

                return View("ConfirmRegistration");
            }
            AddErrors(result);

            return View(model);
        }

        private void NotifyUserManagers(string UserName)
        {
            const Area currentStep = Area.Registration;
            const string lineBreak = "<br>";
            const string qoute = "\"";
            var baseAddress = $"{Request.Url?.Scheme}://{Request.Url?.Authority}{Url.Content("~")}";
            var urlLink = Mailer.GetUrlLink(currentStep, baseAddress, UserName);
            var emailStyleConfig = new EmailStyleConfig();
            var style = emailStyleConfig.Value;

            var messageBody = $"<p style={qoute}{style}{qoute}>" + $"Dear UserManagers,{lineBreak}{lineBreak}" +
                              $"<b>{UserName}</b> is now registered.{lineBreak}{lineBreak}" +
                              $"Click <a href='{urlLink}'>here</a> to grant roles.{lineBreak}{lineBreak}" +
                              "This is an automated message. Do not reply." + "</p>";
            var message = new MailMessage
            {
                Subject = $"TenderSearch {Area.Registration} - {UserName}",
                Body = messageBody
            };
            Mailer.SendEmail(currentStep, message);
        }

        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            try
            {
                if (userId == null || code == null)
                {
                    ViewBag.ErrorMessage = "userId == null || code == null has return true.";
                    return PartialView("Error");
                }
                var result = await UserManager.ConfirmEmailAsync(userId, code);
                return View(result.Succeeded ? "ConfirmEmail" : "Error");
            }
            catch (InvalidOperationException)
            {
                return View("Error");
            }
        }

        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            var user = await UserManager.FindByEmailAsync(model.Email);
            try
            {
                if (!ModelState.IsValid) return View(model);
                if (user == null)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                var code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code }, Request.Url?.Scheme);
                //  await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");

                Mailer.SendEmail(callbackUrl, user.UserName, model.Email);

                return RedirectToAction<AccountController>(c => c.ForgotPasswordConfirmation(), string.Empty);

                // If we got this far, something failed, redisplay form
            }
            catch (Exception ex)
            {
                var methodNamestring =
                    $"{MethodBase.GetCurrentMethod().DeclaringType?.FullName}.{MethodBase.GetCurrentMethod().Name}";
                return GetExceptionErrors(ex, methodNamestring, user?.UserName);
            }
        }
        protected ActionResult GetExceptionErrors(Exception ex, string methodName, string userName)
        {
            if (!Request.IsAjaxRequest()) return ShowError(ex);
            var error = string.Format("<strong>Method:</strong> {1}<br>{0}<br><strong>Inner Exception:</strong> {2}", ex.Message, methodName, ex.InnerException);
            return SetErrorReturnValues(error, userName);
        }
        protected ActionResult ShowError(Exception ex)
        {
            ViewBag.ErrorMessage = ex.Message;
            ViewBag.InnerException = ex.InnerException;
            return View("Error");

        }
        private void LogError(string error)
        {
            Logger.Log(LogLevel.Error, new Exception(error));
        }

        private ActionResult SetErrorReturnValues(string error, string userName)
        {
            LogError(error);
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Content(error, MediaTypeNames.Text.Html);
        }

        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction<AccountController>(c => c.ResetPasswordConfirmation());
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction<AccountController>(c => c.ResetPasswordConfirmation());
            }
            AddErrors(result);
            return View();
        }

        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                ViewBag.ErrorMessage = "User is not verified.";
                return PartialView("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            if (await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
                return RedirectToAction<AccountController>(c => c.VerifyCode(model.SelectedProvider, model.ReturnUrl, model.RememberMe));

            ViewBag.ErrorMessage = "_signInManager.SendTwoFactorCodeAsync returned false.";
            return PartialView("Error");
        }

        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                // return RedirectToAction("Login");
                return RedirectToAction<AccountController>(c => c.Login(returnUrl));
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction<AccountController>(c => c.SendCode(returnUrl, false));
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction<ManageController>(c => c.Index(null));
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction<HomeController>(c => c.Index());
            //return RedirectToAction("Index", "Home", new { area = string.Empty });
        }

        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (UserManager != null)
                {
                    UserManager.Dispose();
                    UserManager = null;
                }

                if (SignInManager != null)
                {
                    SignInManager.Dispose();
                    SignInManager = null;
                }
            }
            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction<HomeController>(c => c.Index());
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri, string userId = null)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}