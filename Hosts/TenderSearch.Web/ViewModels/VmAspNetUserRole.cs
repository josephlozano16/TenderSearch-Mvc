using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using TenderSearch.Business.Common.Entities;
using TenderSearch.Data;
using TenderSearch.Data.Security;

namespace TenderSearch.Web.ViewModels
{
    public class VmAspNetUserRole
    {
        private AspNetUserRole Item { get; set; }

        public VmAspNetUserRole(AspNetUserRole item)
        {
            Item = item;
        }

        public IEnumerable<SelectListItem> GetRoles()
        {
            using (var db = new TenderSearchDb())
            {
                var userStore = new UserStore<ApplicationUser>(db);
                var userManager = new UserManager<ApplicationUser>(userStore);
                var user = userManager.FindByName(Item.UserName);
                var exceptedItems = userManager.GetRoles(user.Id);

                exceptedItems.Remove(Item.Role);

                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
                var Results = roleManager.Roles
                    .OrderBy(x => x.Name)
                    .Where(x => !exceptedItems.Contains(x.Name))
                    .ToList()
                    .Select(x => new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.Name
                    })
                    .ToList();

                Results.Insert(0, new SelectListItem { Text = "- Select - ", Value = "" });
                return Results;
            }
        }

        public bool HasParent()
        {
            return string.IsNullOrWhiteSpace(Item.UserName);
        }
    }
}