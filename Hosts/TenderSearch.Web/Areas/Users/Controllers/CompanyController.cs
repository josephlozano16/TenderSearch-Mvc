using Eml.DataRepository.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Eml.Mediator.Contracts;
using TenderSearch.Business.Common.Entities;
using TenderSearch.Contracts.Dto;
using TenderSearch.Contracts.Infrastructure;
using TenderSearch.Web.Controllers.BaseClasses;

namespace TenderSearch.Web.Areas.Users.Controllers
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [Authorize(Roles = Authorize.Users)]
    public class CompanyController : TableMaintenanceBase<Company>
    {
        [ImportingConstructor]
        public CompanyController(IMediator mediator, IDataRepositorySoftDeleteInt<Company> dataRepository)
            : base(mediator, dataRepository)
        {
        }

        protected override string GetModelTitle(Company item)
        {
            return item.Name;
        }

        protected override async Task<IEnumerable<string>> GetAutoCompleteIntellisenseAsync(int? parentId, string term)
        {
            return await dataRepository
                .GetAutoCompleteIntellisenseAsync(r => r.Name.Contains(term), r => r.OrderBy(y => y.Name), r => r.Name);
        }

        protected override async Task<object> GetPagedListAsync(int? parentId, string searchTerm = null, int page = 1)
        {
            return await dataRepository
                .GetPagedListAsync(page, r => searchTerm == null || r.Name.Contains(searchTerm), r => r.OrderBy(s => s.Name));
        }

        protected override Company FinalizeCreate(Company item)
        {
            return item;
        }

        protected override async Task<UiMessage> IsDuplicateAsync(Company item, string actionName)
        {
            var newValue = item.Name;
            bool hasDuplicates;
            if (actionName == DuplicateNameAction.Create)
                hasDuplicates = await dataRepository.HasDuplicatesAsync(r => !string.IsNullOrEmpty(r.Name)
                                                                             && r.Name.Equals(newValue));
            else
                hasDuplicates = await dataRepository.HasDuplicatesAsync(r => !string.IsNullOrEmpty(r.Name)
                                                                             && r.Name.Equals(newValue)
                                                                             && r.Id != item.Id);

            if (!hasDuplicates) return await Task.FromResult(new UiMessage());

            var cDuplicateMsg = $"Name: <strong>{item.Name}</strong>";

            return await Task.FromResult(new UiMessage(new[] { cDuplicateMsg }));
        }

        protected override string GetParentLabelForEdit(Company item)
        {
            return "Company";
        }

        protected override string GetClassTitle()
        {
            return "Setup Companies";
        }

        protected override void RegisterIDisposables(List<IDisposable> disposables)
        {
            //throw new NotImplementedException();
        }
    }
}
