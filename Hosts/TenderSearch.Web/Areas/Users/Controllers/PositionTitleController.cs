using Eml.DataRepository.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
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
    public class PositionTitleController : TableMaintenanceBase<PositionTitle>
    {
        [ImportingConstructor]
        public PositionTitleController(IMediator mediator, IDataRepositorySoftDeleteInt<PositionTitle> dataRepository)
            : base(mediator, dataRepository)
        {
        }

        protected override string GetModelTitle(PositionTitle item)
        {
            return item.Title;
        }

        protected override async Task<IEnumerable<string>> GetAutoCompleteIntellisenseAsync(int? parentId, string term)
        {
            Expression<Func<PositionTitle, bool>> whereClause = r => r.Title.Contains(term);

            return await dataRepository.GetAutoCompleteIntellisenseAsync(whereClause, 
                r => r.OrderBy(s => s.Title), 
                r => r.Title);
        }

        protected override async Task<object> GetPagedListAsync(int? parentId, string searchTerm = null, int page = 1)
        {
            return await dataRepository
                .GetPagedListAsync(page
                    , r => searchTerm == null || r.Title.Contains(searchTerm)
                    , r => r.OrderBy(s => s.Title));
        }

        protected override PositionTitle FinalizeCreate(PositionTitle item)
        {
            return item;
        }

        protected override async Task<UiMessage> IsDuplicateAsync(PositionTitle item, string actionName)
        {
            var newValue = item.Title;
            bool hasDuplicates;

            if (actionName == DuplicateNameAction.Create)
                hasDuplicates = await dataRepository.HasDuplicatesAsync(r => !string.IsNullOrEmpty(r.Title)
                                                                             && r.Title.Equals(newValue));
            else
                hasDuplicates = await dataRepository.HasDuplicatesAsync(r => !string.IsNullOrEmpty(r.Title)
                                                                             && r.Title.Equals(newValue)
                                                                             && r.Id != item.Id);

            if (!hasDuplicates) return await Task.FromResult(new UiMessage());

            var cDuplicateMsg = $"Name: <strong>{item.Title}</strong>";

            return await Task.FromResult(new UiMessage(new[] { cDuplicateMsg }));
        }

        protected override string GetParentLabelForEdit(PositionTitle item)
        {
            return "PositionTitle";
        }

        protected override string GetClassTitle()
        {
            return "Setup PositionTitle";
        }

        protected override void RegisterIDisposables(List<IDisposable> disposables)
        {
            //throw new NotImplementedException();
        }
    }
}
