using System.Collections.Generic;
using System.Web.Mvc;
using TenderSearch.Business.Common.Entities;
using TenderSearch.Web.ViewModels.BaseClasses;
using TenderSearch.Web.Utils;

namespace TenderSearch.Web.ViewModels
{
    public class VmContactPerson : ViewModelBase<ContactPerson>
    {
        public VmContactPerson(ContactPerson item) : base(item)
        {
        }

        public IEnumerable<SelectListItem> GetCompanies()
        {
            return db.Companies
                .ToSelectListItems(r => r.Id, r => r.Name);
        }

        public IEnumerable<SelectListItem> GetPositionTitles()
        {
            return db.PositionTitles
                .ToSelectListItems(r => r.Id, r => r.Title);
        }

        public static IEnumerable<SelectListItem> GetContractTypes()
        {
            var selectListItems = new List<SelectListItem>
            {
                new SelectListItem{Text =  Contracts.Infrastructure.Contracts.Master, Value = Contracts.Infrastructure.Contracts.Master},
                new SelectListItem{Text = Contracts.Infrastructure.Contracts.Standard, Value = Contracts.Infrastructure.Contracts.Standard}
            };
            selectListItems.Insert(0, new SelectListItem { Text = "- Select - ", Value = "" });
            return selectListItems;
        }

        public override bool HasParent()
        {
            return Item?.CompanyId > 0;
        }

        public override int? GetParentID()
        {
            return Item.CompanyId;
        }
    }
}