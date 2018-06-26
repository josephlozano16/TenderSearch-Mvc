using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using TenderSearch.Business.Common.Entities;
using TenderSearch.Web.Configurations;
using TenderSearch.Web.Utils;
using TenderSearch.Web.ViewModels.BaseClasses;

namespace TenderSearch.Web.ViewModels
{
    public class VmContract : ViewModelBase<Contract>
    {
        private static int? ContractExpiresSoonCountDown { get; set; }
        public VmContract(Contract item) : base(item)
        {
        }

        public async Task<IEnumerable<SelectListItem>> GetCompaniesAsync()
        {
            return await db.Companies
                .ToSelectListItemsAsync(r => r.Id, r => r.Name);
        }
        public IEnumerable<SelectListItem> GetCompanies()
        {
            return  db.Companies
                .ToSelectListItems(r => r.Id, r => r.Name);
        }

        public override bool HasParent()
        {
            return Item?.CompanyId > 0;
        }

        public override int? GetParentID()
        {
            return Item.CompanyId;
        }

        public static int? GetExpirationCountDown(Contract item)
        {
            if (!ContractExpiresSoonCountDown.HasValue)
            {
                var tmpCountDown =new ContractExpiresSoonCountDownConfig();
                ContractExpiresSoonCountDown = tmpCountDown.Value;
            }
            if (!item.EndDate.HasValue) return null;

            var dateDiff = (int?)(item.EndDate.Value - DateTime.Today).TotalDays;
            return dateDiff > ContractExpiresSoonCountDown ? null : dateDiff;
        }
    }
}