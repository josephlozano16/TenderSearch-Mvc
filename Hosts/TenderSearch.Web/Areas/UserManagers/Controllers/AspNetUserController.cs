using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using TenderSearch.Business.Common.Entities;
using TenderSearch.Contracts.Infrastructure;
using TenderSearch.Data;
using TenderSearch.Data.Security;
using TenderSearch.Web.Controllers.BaseClasses;
using X.PagedList;

namespace TenderSearch.Web.Areas.UserManagers.Controllers
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [Authorize(Roles = Authorize.UserManagers)]
    public class AspNetUserController : UserManagerBase<AspNetUser>
    {
        protected override string GetTitle()
        {
            return "User Registration";
        }
        protected override string GetName(AspNetUser Item)
        {
            return Item.UserName;
        }

        protected override async Task<object> GetAutoCompleteIntellisenseAsync(string ParentId, string term)
        {
            using (var db = new TenderSearchDb())
            {
                var userStore = new UserStore<ApplicationUser>(db);
                var userManager = new UserManager<ApplicationUser>(userStore);
                var models = await userManager.Users
                    .OrderBy(r => r.UserName)
                    .Where(r => r.UserName.Contains(term))
                    .Take(INTELLISENSE_SIZE)
                    .ToListAsync();

                return models.Select(r => new { label = r.UserName });
            }

        }

        protected override async Task<object> GetPagedListAsync(string ParentId, string searchTerm = null, int page = 1)
        {
            using (var db = new TenderSearchDb())
            {
                var userStore = new UserStore<ApplicationUser>(db);
                var userManager = new UserManager<ApplicationUser>(userStore);

                var items = await userManager.Users
                    .OrderBy(r => r.UserName)
                    .Where(r => searchTerm == null || r.UserName.Contains(searchTerm))
                    .ToListAsync();

                var tmpItems = items
                    .Select(r => new AspNetUser {UserName = r.UserName, Email = r.Email})
                    .ToList();

                foreach (var i in tmpItems)
                {
                    var user = await userManager.FindByNameAsync(i.UserName);
                    var roles = await userManager.GetRolesAsync(user.Id);
                    roles.ToList().ForEach(r => i.Roles.Add(r));

                }

                return tmpItems
                    .OrderBy(r => r.HasRole)
                    .ThenBy(r => r.UserName)
                    .ToPagedList(page, PAGE_SIZE);
            }
        }

        protected override void EditItemCommit(AspNetUser item)
        {
            throw new NotImplementedException();

        }

        protected override void CreateItemCommit(AspNetUser item)
        {

            throw new NotImplementedException();
        }

        protected override void DeleteItemCommit(string id, string RoleName)
        {
            using (var db = new TenderSearchDb())
            {
                var userStore = new UserStore<ApplicationUser>(db);
                var userManager = new UserManager<ApplicationUser>(userStore);
                var user = userManager.FindByName(id);

                userManager.Delete(user);
                db.SaveChanges();
            }
        }

        protected override AspNetUser GetItem(string id, string RoleName)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            using (var db = new TenderSearchDb())
            {
                var userStore = new UserStore<ApplicationUser>(db);
                var userManager = new UserManager<ApplicationUser>(userStore);
                var user = userManager.FindByName(id);

                return user != null ? new AspNetUser { UserName = user.UserName } : null;
            }
        }
    }
}