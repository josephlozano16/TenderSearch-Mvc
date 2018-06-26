using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using TenderSearch.Business.Common.Entities;
using TenderSearch.Contracts.Infrastructure;
using TenderSearch.Data;
using TenderSearch.Data.Security;
using TenderSearch.Web.Controllers.BaseClasses;
using TenderSearch.Web.Utils;
using X.PagedList;

namespace TenderSearch.Web.Areas.UserManagers.Controllers
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [Authorize(Roles = Authorize.UserManagers)]
    public class AspNetUserRoleController : UserManagerBase<AspNetUserRole>
    {
        protected override string GetTitle()
        {
            return "User Roles";
        }

        protected override string GetName(AspNetUserRole Item)
        {
            return "Manage Roles";
        }

        protected override async Task<object> GetAutoCompleteIntellisenseAsync(string ParentId, string term)
        {
            var userStore = new UserStore<ApplicationUser>(new TenderSearchDb());
            var userManager = new UserManager<ApplicationUser>(userStore);
            var user = await userManager.FindByNameAsync(ParentId);
            var roles = await userManager.GetRolesAsync(user.Id);

            return roles
                .Where(r => r.IndexOf(term, 0, StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(r => r)
                .Take(INTELLISENSE_SIZE)
                .Select(r => new { label = r });

        }

        protected override async Task<object> GetPagedListAsync(string ParentId, string searchTerm = null, int page = 1)
        {
            var userStore = new UserStore<ApplicationUser>(new TenderSearchDb());
            var userManager = new UserManager<ApplicationUser>(userStore);
            var user = await userManager.FindByNameAsync(ParentId);
            var roles = await userManager.GetRolesAsync(user.Id);

            return roles
                .Where(r => searchTerm == null || r.Contains(searchTerm))
                .OrderBy(r => r)
                .Select(r => new AspNetUserRole { UserName = ParentId, Role = r, Email = user.Email })
                .ToPagedList(page, PAGE_SIZE);
        }


        protected override void EditItemCommit(AspNetUserRole item)
        {
            if (string.IsNullOrEmpty(item.Role))
            {
                throw new Exception("You did not select any Role.");
            }

            if (string.Equals(item.Role, item.OldRole, StringComparison.CurrentCultureIgnoreCase))
            {
                return; //do nothing
            }
            var context = new TenderSearchDb();
            var userStore = new UserStore<ApplicationUser>(context);
            var userManager = new UserManager<ApplicationUser>(userStore);
            var user = userManager.FindByName(item.UserName);
            var roles = userManager.GetRoles(user.Id);

            if (roles != null) userManager.RemoveFromRoles(user.Id, roles.ToArray());
            var identityResult = userManager.AddToRole(user.Id, item.Role);
            if (identityResult.Succeeded) context.SaveChanges();

        }

        protected override void CreateItemCommit(AspNetUserRole item)
        {
            var context = new TenderSearchDb();
            var userStore = new UserStore<ApplicationUser>(context);
            var userManager = new UserManager<ApplicationUser>(userStore);
            var user = userManager.FindByName(item.UserName);
            userManager.AddToRole(user.Id, item.Role);
            context.SaveChanges();
        }

        protected override void DeleteItemCommit(string id, string RoleName)
        {
            var context = new TenderSearchDb();
            var userStore = new UserStore<ApplicationUser>(context);
            var userManager = new UserManager<ApplicationUser>(userStore);
            var user = userManager.FindByName(id);
            userManager.RemoveFromRole(user.Id, RoleName);
            context.SaveChanges();
        }

        protected override AspNetUserRole GetItem(string id, string roleName)
        {
            var user = GetUser(id);
            if (user == null) throw new Exception("User doesn't exist: " + id);

            return new AspNetUserRole { UserName = id, Role = roleName, OldRole = roleName, Email = user.Email };
        }

        protected override string GetParentLabel(string parentId)
        {
            var user = GetUser(parentId);
            ViewBag.ParentSubLabel = user.Email;
            return parentId;
        }
        protected override string GetParentID(AspNetUserRole Item)
        {
            return Item.UserName;
        }
        protected override AspNetUserRole CreateItem(string parentId, string email)
        {

            return new AspNetUserRole { UserName = parentId, Email = email };
        }

        protected override void SendEmail(AspNetUserRole item)
        {
            if (!string.IsNullOrEmpty(item.OldRole))
            {
                var oldRole = item.OldRole;
                if (string.Equals(item.Role, oldRole, StringComparison.CurrentCultureIgnoreCase)) return;
            }

            var urlLink = $"{Request.Url?.Scheme}://{Request.Url?.Authority}{Url.Content("~")}";
            Mailer.SendEmail(item, urlLink);
        }
    }
}