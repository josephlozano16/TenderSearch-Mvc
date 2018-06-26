using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Eml.Contracts.Entities;
using Eml.ExplicitDispose.Mvc;
using Microsoft.AspNet.Identity.Owin;
using TenderSearch.Business.Common.Entities;
using TenderSearch.Contracts.Dto;
using TenderSearch.Contracts.Infrastructure;
using TenderSearch.Web.Utils;
using ControllerBase = Eml.ControllerBase.Mvc.ControllerBase;
using Eml.DataRepository.Contracts;
using Eml.Mediator.Contracts;
using TenderSearch.Web.IdentityConfig;

namespace TenderSearch.Web.Controllers.BaseClasses
{
    [ExplicitDispose]
    public abstract class TableMaintenanceBase<T> : ControllerBase, IDisposeAware
        where T : class, IEntityBase<int>, IEntitySoftdeletableBase, new()
    {
        protected TableMaintenanceBase(IMediator mediator, IDataRepositorySoftDeleteInt<T> dataRepository)
        {
            PAGE_SIZE = dataRepository.PageSize;
            INTELLISENSE_SIZE = dataRepository.IntellisenseSize;

            ViewBag.APPVERSION = APPVERSION;

            this.dataRepository = dataRepository;
            this.mediator = mediator;
        }

        #region MEMBERS
        protected readonly IMediator mediator;

        protected readonly IDataRepositorySoftDeleteInt<T> dataRepository;

        public string APPVERSION { get; } = GetApplicationVersion();

        protected int PAGE_SIZE { get; private set; }

        protected int INTELLISENSE_SIZE { get; private set; }

        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get => _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            private set => _userManager = value;
        }
        #endregion //MEMBERS

        #region OVERRIDABLES
        protected abstract string GetModelTitle(T item);

        protected abstract Task<IEnumerable<string>> GetAutoCompleteIntellisenseAsync(int? parentId, string term);

        protected abstract Task<object> GetPagedListAsync(int? parentId, string searchTerm = null, int page = 1);

        /// <summary>
        /// Override this if you need to include Foreign Rows.
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        protected virtual async Task<T> FindItemAsync(int Id) //Overrides by children. Unused by flat table or parent table
        {
            return await dataRepository.GetAsync(Id);
        }

        /// <summary>
        /// Override this if you have to set parent related fields such as parent ID and Parent Entity. Happens during GET.
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        protected virtual async Task<T> CreateItemAsync(int parentId)
        {
            return await Task.FromResult<T>(null);
        }

        /// <summary>
        /// Override this if you have to set parent related fields such as parent ID and Parent Entity. Happens during POST.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected abstract T FinalizeCreate(T item);

        protected abstract Task<UiMessage> IsDuplicateAsync(T item, string actionName);

        //abstract protected void TrimValues(T item);
        //
        /// <summary>
        /// Override this if you have other business logic to implement such  as cascade of of L_* tables to Txn_* tables. Happens during POST.
        /// </summary>
        /// <param name="item"></param>
        protected virtual void FinalizeBusinessLogicForCreate(T item)
        {
            return;
        }

        protected virtual void FinalizeEdit(T item)//Overrides by children.
        {
            return;
        }

        protected virtual void FinalizeSort(T item)//Overrides by children.
        {
            return;
        }

        protected virtual void SortChildren(T item)
        {
            return;
        }

        protected virtual async Task<string> GetParentLabelAsync(int parentId)//Overrides by children.
        {
            return await Task.FromResult(string.Empty);
        }

        protected abstract string GetParentLabelForEdit(T item); //Overrides by children.

        protected virtual int? GetParentID(T Item)//Overrides by children.
        {

            return null;
        }

        protected virtual string GetClassName(T item)
        {
            return typeof(T).Name;
        }

        protected abstract string GetClassTitle(); //used in Table Titles

        protected virtual string GetClassName()
        {
            return typeof(T).Name; //used in pagedList Identification
        }

        protected virtual string GetSpaceDelimitedWords(string word)
        {
            return Regex.Replace(word, "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", "$1 ");
        }

        protected virtual void SendEmail(Area FromStage, MailMessage message)
        {
            Mailer.SendEmail(FromStage, message);
        }

        protected virtual void SendEmail(T item)
        {
            return;
        }

        protected virtual T CopyPrepare(T item)
        {
            return item;
        }

        protected virtual T CopyItem(T item)
        {
            return item;
        }

        protected string GetUrlLink(Area currentStep, int Id)
        {
            var baseAddress = $"{Request.Url?.Scheme}://{Request.Url?.Authority}{Url.Content("~")}";
            var urlLink = Mailer.GetUrlLink(currentStep, baseAddress, Id.ToString());

            return urlLink;
        }

        protected string GetMessageBody(Contract item, string urlLink)
        {
            return Mailer.GetMessageBody(item, urlLink);
        }
        #endregion //OVERRIDABLES

        #region PUBLIC METHODS

        #region UTILITIES

        #region Error Handlers
        protected ActionResult GetModelStateErrors(ModelStateDictionary modelState, string methodName)
        {
            var errors = modelState.Values.SelectMany(v => v.Errors).ToList();
            var cErrors = errors.ConvertAll(r => r.ErrorMessage);
            var uiMessage = new UiMessage(methodName, cErrors);

            return Request.IsAjaxRequest() ? SetErrorReturnValues(uiMessage) : ShowError(uiMessage);
        }

        protected ActionResult GetValidationErrors(DbEntityValidationException dbEx, string methodName)
        {
            var errors = new List<string>();

            foreach (var validationErrors in dbEx.EntityValidationErrors)
            {
                errors.AddRange(validationErrors.ValidationErrors.Select(r => $"Property: <strong>{r.PropertyName}</strong> Message: <strong>{r.ErrorMessage}</strong>"));
            }

            var uiMessage = new UiMessage(methodName, errors);

            return Request.IsAjaxRequest() ? SetErrorReturnValues(uiMessage) : ShowError(uiMessage);
        }

        protected ActionResult GetExceptionErrors(Exception ex, string methodName)
        {
            var uiMessage = new UiMessage(methodName, new[] { $"{ex.Message}" });

            if (ex.InnerException != null)
            {
                var innerException = ex.InnerException.ToString();

                uiMessage = new UiMessage(methodName, new[] { $"{ex.Message}<br><strong>Inner Exception:</strong> {innerException}" });
            }

            return Request.IsAjaxRequest() ? SetErrorReturnValues(uiMessage) : ShowError(uiMessage);
        }

        private static void LogError(UiMessage uiMessage)
        {
            MvcApplication.Logger.Log.Error(new Exception(uiMessage.GetMessages()));
        }

        private static void LogError(Exception exception)
        {
            MvcApplication.Logger.Log.Error(exception);
        }

        private ActionResult SetErrorReturnValues(UiMessage uiMessage)
        {
            try
            {
                dataRepository.DiscardChanges();
                LogError(uiMessage);
            }
            catch (Exception)
            {
                // ignored
            }

            Response.StatusCode = (int)HttpStatusCode.BadRequest;

            return Content(uiMessage.GetHtmlMessages(), MediaTypeNames.Text.Html);
        }
        #endregion // Error Handlers

        public async Task<ActionResult> AutoComplete(int? ParentId, string term)
        {
            try
            {
                var results = await GetAutoCompleteIntellisenseAsync(ParentId, term);
                var model = results.Select(r => new { label = r });

                return Json(model, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                const string methodNamestring = "AutoComplete";

                return GetExceptionErrors(ex, methodNamestring);
            }
        }

        public ActionResult Goto(string ReturnUrl)
        {
            return Redirect(ReturnUrl);
        }

        public ActionResult GetChildren(int? ParentId, string ReturnToParentListURL, string subCategory) //, string ParentLabel
        {
            const string action = "Index";

            var controller = "";
            var typeName = typeof(T).Name;

            if (typeName == "Company")
            {
                if (subCategory == "ContactPerson") controller = "ContactPerson";
                if (subCategory == "Contract") controller = "Contract";
            }

            return RedirectToAction(action, controller, new { ParentId = ParentId, ReturnToParentListURL = ReturnToParentListURL });
        }

        public UiMessage GetDuplicateMessage(UiMessage uiMessage, string methodName)
        {
            var tmpMessages = uiMessage.GetPrivateMessages();

            tmpMessages.Insert(0, "Duplicate is not allowed.<br><br>");
            tmpMessages.Add("<br><br>Please try again.");

            return new UiMessage(methodName, tmpMessages);
        }
        #endregion // UTILITIES

        #region DERIVED FROM SCAFFOLDINGS

        #region Index
        public async Task<ActionResult> Index(int? ParentId, string searchTerm = null, int page = 1, string ReturnToParentListURL = null)
        {
            var model = await GetPagedListAsync(ParentId, searchTerm, page);
            var typeName = typeof(T).Name;//used in pagedList Identification

            ViewBag.Title = GetClassTitle();
            ViewBag.ReturnUrl = string.IsNullOrWhiteSpace(ReturnToParentListURL) ? Request.Url?.AbsoluteUri : ReturnToParentListURL;
            ViewBag.ParentId = ParentId;
            ViewBag.searchTerm = searchTerm;
            ViewBag.PageNumber = page;
            ViewBag.PAGE_SIZE = PAGE_SIZE;
            ViewBag.ReturnToParentListURL = ReturnToParentListURL;
            ViewBag.TargetTableBody = $"paged{typeName}body";

            var user = await UserManager.FindByNameAsync(User.Identity.Name);
            var rolesForUser = await UserManager.GetRolesAsync(user.Id);

            ViewBag.RoleCount = rolesForUser.Count;

            if (Request.IsAjaxRequest()) return PartialView("_LayoutIndexContentsAjax", model);
            if (ParentId.HasValue && ParentId.Value > 0) ViewBag.ParentLabel = await GetParentLabelAsync(ParentId.Value);

            return View(model);
        }
        #endregion // Index

        #region Details
        public async Task<ActionResult> Details(int? id, string ReturnUrl)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var item = await dataRepository.GetAsync(id.Value);

            if (item == null) return HttpNotFound();

            ViewBag.Title = "Details";
            ViewBag.ReturnUrl = ReturnUrl;

            var modelType = GetClassName(item); // typeof(T).Name;  //GetModelTitle

            modelType = GetSpaceDelimitedWords(modelType);
            ViewBag.ModelType = modelType;
            ViewBag.ID = id;

            return View(item);
        }
        #endregion // Details

        #region Create
        public async Task<ActionResult> Create(int? ParentId, string ReturnUrl) //, string ParentLabel
        {
            return await DoCreateAsync(ParentId, ReturnUrl, CreateItemAsync, "_LayoutCreateAjax");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(T item, string ReturnUrl)
        {
            const string methodNamestring = "POST/Create";

            try
            {
                if (ModelState.IsValid)
                {
                    var actionName = GetActionName();

                    item.TrimStringValues();
                    var uiMessage = await IsDuplicateAsync(item, actionName);

                    if (uiMessage.Any)
                    {
                        uiMessage = GetDuplicateMessage(uiMessage, "Create");

                        return Request.IsAjaxRequest() ? SetErrorReturnValues(uiMessage) : ShowError(uiMessage);
                    }

                    return await DoCreateConfirmAsync(item, ReturnUrl, FinalizeCreate, SendEmail, FinalizeBusinessLogicForCreate);
                }

                return GetModelStateErrors(ModelState, methodNamestring);
            }
            catch (DbEntityValidationException dbEx)
            {
                return GetValidationErrors(dbEx, methodNamestring);
            }
            catch (Exception ex)
            {
                return GetExceptionErrors(ex, methodNamestring);
            }
        }

        private async Task<ActionResult> DoCreateAsync(int? parentId, string returnUrl, Func<int, Task<T>> createAsync, string partialViewName)
        {
            try
            {
                var classTitle = GetClassTitle(); //typeof(T1).Name;

                SetViewBagsForCreate(returnUrl, classTitle);

                if (parentId.HasValue)
                {
                    var item = await createAsync(parentId.Value);

                    if (Request.IsAjaxRequest()) return PartialView(partialViewName, item);

                    return View(item);
                }

                if (Request.IsAjaxRequest()) return PartialView(partialViewName);

                return View();
            }
            catch (Exception ex)
            {
                const string methodNamestring = "DoCreateAsync";

                return GetExceptionErrors(ex, methodNamestring);
            }
        }

        private async Task<ActionResult> DoCreateConfirmAsync(T item, string returnUrl, Func<T, T> finalize, Action<T> SendMail, Action<T> businessLogic)
        {
            const string methodNamestring = "DoCreateAsync";
            try
            {
                if (ModelState.IsValid)
                {
                    if (finalize != null)
                    {
                        var item1 = item;

                        item = await Task.Run(() => finalize(item1));//override this if you have to set parent related fields to populate
                    }

                    await dataRepository.AddAsync(item);

                    var tmpItem1 = item;

                    await Task.Run(() => businessLogic?.Invoke(tmpItem1));
                    await Task.Run(() => SendMail?.Invoke(tmpItem1));

                    return Request.IsAjaxRequest() ? Content("") : Goto(returnUrl);
                }

                return GetModelStateErrors(ModelState, methodNamestring);
            }
            catch (DbEntityValidationException dbEx)
            {
                return GetValidationErrors(dbEx, methodNamestring);
            }
            catch (Exception ex)
            {
                return GetExceptionErrors(ex, methodNamestring);
            }
        }

        private void SetViewBagsForCreate(string returnUrl, string classTitle)
        {
            //  modelType = GetSpaceDelimitedWords(modelType);///Create space separated words here
            // string modelType , string modelType
            ViewBag.Title = "Create";
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.ModelType = classTitle;// modelType;
            ViewBag.PAGE_SIZE = PAGE_SIZE;
        }
        #endregion // Create

        #region Edit
        public async Task<ActionResult> Edit(int? id, string ReturnUrl)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var item = await FindItemAsync(id.Value);

            if (item == null) return HttpNotFound();

            SortChildren(item);
            SetViewBagsForEdit(item, ReturnUrl);

            if (Request.IsAjaxRequest()) return PartialView("_LayoutEditAjax", item);

            return View(item);
        }

        protected void SetViewBagsForEdit(T item, string returnUrl)
        {
            ViewBag.Title = "Edit";
            ViewBag.ReturnUrl = returnUrl;

            var modelTitle = GetModelTitle(item);

            modelTitle = GetSpaceDelimitedWords(modelTitle);

            var modelType = GetParentLabelForEdit(item); //typeof(T).Name; //TODO Get parent Title

            ViewBag.ModelTitle = modelTitle;
            ViewBag.ModelType = modelType;
            ViewBag.PAGE_SIZE = PAGE_SIZE;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(T item, string ReturnUrl)
        {
            const string methodNamestring = "POST/Edit";

            try
            {
                if (!ModelState.IsValid) return GetModelStateErrors(ModelState, methodNamestring);

                dataRepository.SetModified(item);
                item.TrimStringValues();

                var actionName = GetActionName();
                var uiMessage = await IsDuplicateAsync(item, actionName);

                if (uiMessage.Any)
                {
                    uiMessage = GetDuplicateMessage(uiMessage, "Edit");

                    return Request.IsAjaxRequest() ? SetErrorReturnValues(uiMessage) : ShowError(uiMessage);
                }

                FinalizeEdit(item);

                await dataRepository.SaveChangesAsync();

                return Request.IsAjaxRequest() ? Content("") : Goto(ReturnUrl);
            }
            catch (DbEntityValidationException dbEx)
            {
                return GetValidationErrors(dbEx, methodNamestring);
            }
            catch (Exception ex)
            {
                return GetExceptionErrors(ex, methodNamestring);
            }
        }

        #endregion // Edit

        #region Delete
        public ActionResult Delete(int? id, string ReturnUrl)
        {
            return DoDelete(id, ReturnUrl, GetModelTitle, GetClassName);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(T Item, string ReturnUrl)
        {
            return DoDeleteConfirmed(Item, ReturnUrl, true, null);
        }

        //protected string GetOldDeletionReason(DbSet items, T item)
        //{
        //    var id = item.Id;
        //    var tmpItem = dataRepository.Get(id);// dbset?.FirstOrDefault(r => r.Id == id);
        //    var itemAsModel_Base = tmpItem;
        //    var oldDeletionReason = itemAsModel_Base?.DeletionReason;

        //    return oldDeletionReason;
        //}
        //protected string GetOldDeletionReason(T item)
        //{
        //    var oldDeletionReason = "";
        //    oldDeletionReason = item.DeletionReason;

        //    return oldDeletionReason;
        //}

        protected string GetNewDeletionReason(T item, DateTime timeStamp)
        {
            var newDeletionReason = $"{timeStamp}\t{User.Identity.Name}:\t{item.DeletionReason}";

            return newDeletionReason;
        }

        protected string GetDeletionMessages(string oldDeletionReason, string newDeletionReason)
        {
            var msg = string.IsNullOrWhiteSpace(oldDeletionReason)
                ? newDeletionReason
                : $"{oldDeletionReason}{Environment.NewLine}{newDeletionReason}";

            return msg;
        }

        protected ActionResult DoDeleteConfirmed(T item, string returnUrl, bool allowImmediateCommit,
            Action<T, string, string, DateTime> finalizeDelete)
        {
            var itemFromDB = dataRepository.Get(item.Id);

            if (itemFromDB == null) return HttpNotFound();

            var timeStamp = DateTime.UtcNow;
            var oldDeletionReason = itemFromDB.DeletionReason; //GetOldDeletionReason(Items, item);
            var newDeletionReason = GetNewDeletionReason(item, timeStamp);

            finalizeDelete?.Invoke(itemFromDB, returnUrl, newDeletionReason, timeStamp);

            return DoDeleteConfirmed(itemFromDB, returnUrl, oldDeletionReason, newDeletionReason, timeStamp, allowImmediateCommit);
        }

        protected ActionResult DoDeleteConfirmed(T itemFromDB, string returnUrl,
            string oldDeletionReason,
            string newDeletionReason,
            DateTime timeStamp,
            bool allowImmediateCommit)
        {
            if (itemFromDB == null) return HttpNotFound();

            try//soft deletes
            {
                var msg = GetDeletionMessages(oldDeletionReason, newDeletionReason);

                dataRepository.SetUnchanged(itemFromDB);

                itemFromDB.DateDeleted = timeStamp;
                itemFromDB.DeletionReason = msg;

                if (allowImmediateCommit) dataRepository.SaveChanges();
                if (!Request.IsAjaxRequest()) return allowImmediateCommit ? Goto(returnUrl) : null;

                return allowImmediateCommit ? Content(bool.TrueString) : null;
            }
            catch (Exception ex)
            {
                const string methodNamestring = "DoDeleteConfirmed";

                return GetExceptionErrors(ex, methodNamestring);
            }
        }

        protected ActionResult DoDelete(int? id, string returnUrl, Func<T, string> getModelTitle, Func<T, string> getClassName)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var item = dataRepository.Get(id.Value);

            if (item == null) return HttpNotFound();

            item.DeletionReason = string.Empty; //clear only (no save) for display purposes

            var modelTitle = getModelTitle(item);
            var modelType = getClassName(item);

            ViewBag.ReturnUrl = returnUrl;

            if (Request.IsAjaxRequest()) return PartialView("_LayoutDeleteSubmitButtonAjax", item);

            modelTitle = GetSpaceDelimitedWords(modelTitle);
            modelType = GetSpaceDelimitedWords(modelType);

            ViewBag.Title = "Delete";
            ViewBag.ModelTitle = modelTitle;
            ViewBag.ModelType = modelType;

            return View(item);
        }

        #endregion // Delete

        #endregion //DERIVED FROM SCAFFOLDINGS

        #region STATIC

        public static string GetApplicationVersion()
        {
            var executingAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            var version = executingAssembly.GetName().Version;

            return version != null ? $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}" : string.Empty;

        }
        #endregion // STATIC

        #endregion //PUBLIC METHODS

        #region PROTECTED METHODS
        protected ActionResult ShowError(UiMessage uiMessage)
        {
            ViewBag.ErrorMessage = uiMessage.GetHtmlMessages();
            LogError(uiMessage);

            return View("Error");
        }
        #endregion // PROTECTED METHODS

        private string GetActionName()
        {
            var routeValues = Request.RequestContext.RouteData.Values;

            if (routeValues.ContainsKey("action"))
                return (string)routeValues["action"];

            return string.Empty;
        }

        /// <summary>
        /// Ex: Disposables.Add(Concrete classes that implements IDisposable);
        /// </summary>
        /// <param name="disposables"></param>
        protected abstract void RegisterIDisposables(List<IDisposable> disposables);

        public void RegisterDisposables(List<IDisposable> disposables)
        {
            Disposables.Add(dataRepository);
            RegisterIDisposables(disposables);
        }

        public List<IDisposable> Disposables { get; } = new List<IDisposable>();
    }
}