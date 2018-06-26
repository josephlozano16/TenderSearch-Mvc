using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Eml.DataRepository;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using NLog;
using TenderSearch.Data;
using TenderSearch.Data.Security;
using TenderSearch.Web.IdentityConfig;
using LogLevel = NLog.LogLevel;
using ControllerBase = Eml.ControllerBase.Mvc.ControllerBase;

namespace TenderSearch.Web.Controllers.BaseClasses
{
    public abstract class UserManagerBase<T> : ControllerBase
        where T : class
    {
        protected UserManagerBase()
        {
            var  pageSizeConfig = new PageSizeConfig();
            var intellisenseCount = new IntellisenseCountConfig();

            PAGE_SIZE = pageSizeConfig.Value;
            INTELLISENSE_SIZE = intellisenseCount.Value;
        }

        #region MEMBERS
        protected int PAGE_SIZE { get; private set; }
        protected int INTELLISENSE_SIZE { get; private set; }
        private ApplicationUserManager _userManager;

        private ApplicationUserManager UserManager
        {
            get => _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            set => _userManager = value;
        }

        private Logger Logger { get; set; }
        #endregion //MEMBERS

        #region OVERRIDABLE
        protected abstract string GetTitle();
        protected abstract string GetName(T Item);
        protected abstract Task<object> GetAutoCompleteIntellisenseAsync(string ParentId, string term);
        protected abstract Task<object> GetPagedListAsync(string ParentId, string searchTerm = null, int page = 1);
        protected abstract void EditItemCommit(T item);
        protected abstract void CreateItemCommit(T item);
        protected abstract void DeleteItemCommit(string id, string RoleName);
        protected abstract T GetItem(string id, string RoleName);
        protected virtual T CreateItem(string ParentId, string Email) //Overrides by children. Unused by flat table or parent table
        {
            return null;
        }

        protected virtual string GetParentLabel(string ParentId)//Overrides by children.
        {
            return "";
        }

        protected virtual string GetParentID(T Item)//Overrides by children.
        {
            return null;
        }

        private ActionResult GetExceptionErrors(Exception ex, string methodName)
        {
            if (!Request.IsAjaxRequest()) return ShowError(ex);
            var error = $"<strong>Method:</strong> {methodName}<br>{ex.Message}<br><strong>Inner Exception:</strong> {ex.InnerException}";
            return SetErrorReturnValues(error);
        }

        private ActionResult GetModelStateErrors(ModelStateDictionary modelState, string methodName)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).ToList();
            var cErrors = errors.ConvertAll(r => r.ErrorMessage);
            var newLine = Request.IsAjaxRequest() ? "<br>" : Environment.NewLine;
            var error = string.Join(newLine, cErrors.ToArray());

            if (Request.IsAjaxRequest())
            {
                return SetErrorReturnValues(error);
            }

            var ex = new Exception(error);
            return ShowError(ex);
        }
        protected ActionResult ShowError(Exception ex)
        {
            ViewBag.ErrorMessage = ex.Message;
            ViewBag.InnerException = ex.InnerException;
            return View("Error");

        }
        protected ActionResult GetValidationErrors(DbEntityValidationException dbEx, string methodName)
        {
            var errors = new List<string>();
            foreach (var validationErrors in dbEx.EntityValidationErrors)
            {
                errors.AddRange(validationErrors.ValidationErrors.Select(validationError => $"Property: {validationError.PropertyName} Message: {validationError.ErrorMessage}"));
            }
            if (!Request.IsAjaxRequest()) return ShowError(dbEx);
            errors.Add("Method: " + methodName);
            var error = string.Join("<br>", errors.ToArray());

            return SetErrorReturnValues(error);
        }
        private void LogError(string error)
        {
            Logger.Log(LogLevel.Error, new Exception(error));

        }
        private ActionResult SetErrorReturnValues(string error)
        {
            LogError(error);
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Content(error, MediaTypeNames.Text.Html);

        }
        protected ApplicationUser GetUser(string UserName)
        {
            using (var db = new TenderSearchDb())
            {
                var userStore = new UserStore<ApplicationUser>(db);
                var userManager = new UserManager<ApplicationUser>(userStore);

                var user = userManager.FindByName(UserName);
                return user;
            }
        }
        #endregion // OVERRIDABLE

        #region PUBLIC METHODS
        public async Task<ActionResult> AutoComplete(string ParentId, string term)
        {
            try
            {
                var model = await GetAutoCompleteIntellisenseAsync(ParentId, term);
                return Json(model, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var methodNamestring =
                    $"{MethodBase.GetCurrentMethod().DeclaringType?.FullName}.{MethodBase.GetCurrentMethod().Name}";
                return GetExceptionErrors(ex, methodNamestring);
            }
        }

        public ActionResult Goto(string ReturnUrl)
        {
            return Redirect(ReturnUrl);

        }

        public ActionResult GetChildren(string ParentId, string ReturnToParentListURL) //, string ParentLabel
        {
            var controller = "";
            if (typeof(T).Name == "AspNetUser")//parent
            {
                controller = "AspNetUserRole";//child
            }
            return RedirectToAction("Index", controller, new { ParentId = ParentId, ReturnToParentListURL = ReturnToParentListURL });
        }

        #region INDEX
        public async Task<ActionResult> Index(string ParentId, string searchTerm = null, int page = 1, string ReturnToParentListURL = null)
        {
            var model = await GetPagedListAsync(ParentId, searchTerm, page);
            var title = GetTitle(); // typeof(T).Name;

            title = GetSpaceDelimitedWords(title);

            ViewBag.Title = title; //+ "s"; //pluralize
            ViewBag.ReturnUrl = Request.Url?.AbsoluteUri; //This is the only place where we should modify the value _LayoutIndexContents
            ViewBag.ParentId = ParentId;
            ViewBag.searchTerm = searchTerm;
            ViewBag.PageNumber = page;
            ViewBag.PAGE_SIZE = PAGE_SIZE;
            ViewBag.ReturnToParentListURL = ReturnToParentListURL;

            var user = await UserManager.FindByNameAsync(User.Identity.Name);
            var rolesForUser = await UserManager.GetRolesAsync(user.Id);
            ViewBag.RoleCount = rolesForUser.Count;

            if (Request.IsAjaxRequest()) return PartialView("_LayoutIndexContentsAjax", model);
            if (!string.IsNullOrWhiteSpace(ParentId)) ViewBag.ParentLabel = GetParentLabel(ParentId);
            return View(model);
        }
        #endregion // INDEX

        #region DETAILS
        public ActionResult Details(string id, string ReturnUrl)
        {

            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            //ApplicationUser user = GetUser(id);
            //if (user == null) throw new Exception("User doesn't exist: " + id);

            var item = GetItem(id, ""); //TODO find role
            if (item == null) return HttpNotFound();

            ViewBag.Title = "Details";
            ViewBag.ReturnUrl = ReturnUrl;

            var modelType = typeof(T).Name;

            modelType = GetSpaceDelimitedWords(modelType);
            ViewBag.ModelType = modelType;
            ViewBag.ID = id;

            return View(item);
        }
        #endregion // DETAILS

        #region CREATE
        public ActionResult Create(string ParentId, string ReturnUrl) //, string ParentLabel
        {
            try
            {
                SetViewBagsForCreate(ParentId, ReturnUrl);

                if (!string.IsNullOrWhiteSpace(ParentId))
                {
                    var user = GetUser(ParentId);
                    if (user == null) throw new Exception("User doesn't exist: " + ParentId);

                    var item = CreateItem(ParentId, user.Email);

                    if (Request.IsAjaxRequest()) return PartialView("_LayoutCreateAjax", item);
                    return View(item);
                }

                if (Request.IsAjaxRequest()) return PartialView("_LayoutCreateAjax");
                return View();
            }
            catch (Exception ex)
            {
                var methodNamestring =
                    $"{MethodBase.GetCurrentMethod().DeclaringType?.FullName}.{MethodBase.GetCurrentMethod().Name}";
                return GetExceptionErrors(ex, methodNamestring);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(T item, string ReturnUrl)
        {
            if (ModelState.IsValid)
            {
                CreateItemCommit(item);
                SendEmail(item);
                return Request.IsAjaxRequest() ? Content(ReturnUrl) : Goto(ReturnUrl);
            }
            else
            {
                var methodNamestring =
                    $"{MethodBase.GetCurrentMethod().DeclaringType?.FullName}.{MethodBase.GetCurrentMethod().Name}";
                return GetModelStateErrors(ModelState, methodNamestring);
            }
        }

        protected virtual void SendEmail(T item) //overrides by children: AspNetUserRole
        {
            return;
        }

        #endregion // CREATE

        #region EDIT
        public ActionResult Edit(string UserName, string RoleName, string ReturnUrl)
        {
            if (UserName == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var item = GetItem(UserName, RoleName);//Items.Find(id);

            if (item == null) return HttpNotFound();

            SetViewBagsForEdit(item, ReturnUrl);

            if (Request.IsAjaxRequest()) return PartialView("_LayoutEditAjax", item);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(T item, string ReturnUrl)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    EditItemCommit(item);
                    SendEmail(item);
                    return Request.IsAjaxRequest() ? Content(ReturnUrl) : Goto(ReturnUrl);
                }
                else
                {
                    var methodNamestring =
                        $"{MethodBase.GetCurrentMethod().DeclaringType?.FullName}.{MethodBase.GetCurrentMethod().Name}";
                    return GetModelStateErrors(ModelState, methodNamestring);
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                var methodNamestring =
                    $"{MethodBase.GetCurrentMethod().DeclaringType?.FullName}.{MethodBase.GetCurrentMethod().Name}";
                return GetValidationErrors(dbEx, methodNamestring);
            }
            catch (Exception ex)
            {
                var methodNamestring =
                    $"{MethodBase.GetCurrentMethod().DeclaringType?.FullName}.{MethodBase.GetCurrentMethod().Name}";
                return GetExceptionErrors(ex, methodNamestring);
            }
        }
        #endregion // EDIT

        #region DELETE
        public ActionResult Delete(string UserName, string RoleName, string ReturnUrl) //renamed the parameter from 'id' to 'UserName'  bec id is always treated as integer by MVC shit.
        {
            if (UserName == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var item = GetItem(UserName, RoleName);
            if (item == null) return HttpNotFound();

            ViewBag.UserName = UserName;
            ViewBag.RoleName = RoleName;
            ViewBag.ReturnUrl = ReturnUrl;

            if (Request.IsAjaxRequest()) return PartialView("_LayoutDeleteSubmitButtonAjax", item);

            var modelTitle = GetName(item);
            modelTitle = GetSpaceDelimitedWords(modelTitle);
            ViewBag.Title = "Delete";

            ViewBag.ModelTitle = modelTitle;


            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string UserName, string RoleName, string ReturnUrl)
        {
            try
            {
                DeleteItemCommit(UserName, RoleName);
                return Request.IsAjaxRequest() ? Content(bool.TrueString) : Goto(ReturnUrl);
            }
            catch (Exception ex)
            {
                var methodNamestring =
                    $"{MethodBase.GetCurrentMethod().DeclaringType?.FullName}.{MethodBase.GetCurrentMethod().Name}";
                return GetExceptionErrors(ex, methodNamestring);
            }
        }
        #endregion // DELETE

        #endregion // PUBLIC METHODS

        #region PRIVATE METHODS
        private static string GetSpaceDelimitedWords(string word)
        {
            return Regex.Replace(word, "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", "$1 ");
        }
        private void SetViewBagsForEdit(T item, string ReturnUrl)
        {
            ViewBag.Title = "Edit";
            ViewBag.ReturnUrl = ReturnUrl;

            var modelTitle = GetName(item);
            modelTitle = GetSpaceDelimitedWords(modelTitle);

            var modelType = typeof(T).Name;
            modelType = GetSpaceDelimitedWords(modelType);

            ViewBag.ModelTitle = modelTitle;
            ViewBag.ModelType = modelType;


            //  ViewBag.CurrentRole = GetRole(item);
        }
        private void SetViewBagsForCreate(string ParentId, string ReturnUrl)
        {
            var modelType = typeof(T).Name;
            modelType = GetSpaceDelimitedWords(modelType);//Create space separated words here

            ViewBag.Title = "Create";
            ViewBag.ReturnUrl = ReturnUrl;
            ViewBag.ModelType = modelType;
            ViewBag.ParentId = ParentId;
        }

        #endregion // PRIVATE METHODS

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
            base.Dispose(disposing);
        }
    }
}