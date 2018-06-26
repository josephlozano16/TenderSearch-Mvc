using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using TenderSearch.Business.Common.Entities;
using TenderSearch.Contracts.Dto;
using TenderSearch.Contracts.Infrastructure;
using TenderSearch.Web.Controllers.BaseClasses;
using Eml.DataRepository.Contracts;
using System.Linq.Expressions;
using Eml.ClassFactory.Contracts.Extensions;
using Eml.Mediator.Contracts;
using TenderSearch.Business.Requests;

namespace TenderSearch.Web.Areas.Users.Controllers
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [Authorize(Roles = Authorize.Users)]
    public class ContractController : TableMaintenanceChildBase<Contract>
    {
        private readonly IDataRepositorySoftDeleteInt<Company> _companyRepository;

        [ImportingConstructor]
        public ContractController(IMediator mediator, IDataRepositorySoftDeleteInt<Contract> dataRepository, IDataRepositorySoftDeleteInt<Company> companyRepository)
            : base(mediator, dataRepository)
        {
            _companyRepository = companyRepository;
        }

        protected override string GetModelTitle(Contract item)
        {
            return item.Company.Name;
        }

        protected override async Task<IEnumerable<string>> GetAutoCompleteIntellisenseAsync(int? parentId, string term)
        {
            Expression<Func<Contract, bool>> whereClause = r => r.ContractType.Contains(term);

            if (parentId.HasValue)
            {
                whereClause = r => r.CompanyId == parentId
                                   && r.ContractType.Contains(term);
            }

            return await dataRepository.GetAutoCompleteIntellisenseAsync(r => r.Include(s => s.Company),
                whereClause, 
                r => r.OrderBy(s => s.ContractType), 
                r => r.ContractType);
        }

        protected override async Task<object> GetPagedListAsync(int? parentId, string searchTerm = null, int page = 1)
        {
            if (parentId.HasValue)
            {
                return await dataRepository
                    .GetPagedListAsync(page
                        , r => r.Include(s => s.Company)
                        , r => r.CompanyId == parentId
                               && (searchTerm == null || r.ContractType.Contains(searchTerm))
                        , r => r.OrderBy(s => s.EndDate)
                                .ThenBy(s => s.ContractType));
            }
            return await dataRepository
                .GetPagedListAsync(page
                , r => r.Include(s => s.Company)
                , r => searchTerm == null || r.ContractType.Contains(searchTerm)
                , r => r.OrderBy(s => s.Company.Name)
                        .ThenBy(s => s.EndDate)
                        .ThenBy(s => s.ContractType));
        }

        protected override Contract FinalizeCreate(Contract item)
        {
            item.Company = null; //prevent EF6 from inserting new parent

            return item;
        }

        protected override async Task<UiMessage> IsDuplicateAsync(Contract item, string actionName)
        {
            const string htmlTagWithNoPairToReplace = "<hr>";

            var request = new DuplicateContractAsyncRequest(actionName, item.CompanyId, item.Id, item.RenewalDate, item.EndDate);
            var response = await mediator.GetAsync(request);
            var models = response.Contracts;
            var messages = new List<string>();
            var lineBreak = Environment.NewLine;

            models.ForEach(r =>
            {
                messages.Add($"ContractType: <strong>{r.ContractType}</strong>" +
                             $"{lineBreak}RenewalDate: <strong>{r.RenewalDate.ToStringOrDefault("d")}</strong>" +
                             $"{lineBreak}EndDate: <strong>{r.EndDate.ToStringOrDefault("d")}</strong>");
            });

            var htmlTagsWithNoPairToReplace = UiMessage.GetHtmlTagsToReplace(new[] { htmlTagWithNoPairToReplace });
            var uiMessage = new UiMessage(messages, htmlTagsWithNoPairToReplace);

            return await Task.FromResult(uiMessage);
        }

        protected override string GetParentLabelForEdit(Contract item)
        {
            return item.Company.Name;
        }

        protected override string GetClassTitle()
        {
            return "Setup Contracts ";
        }

        protected override async Task<string> GetParentLabelAsync(int parentId)
        {
            return await dataRepository.GetValueAsync(r => r.CompanyId == parentId, r => r.Company.Name);
        }

        protected override void FinalizeEdit(Contract item)
        {
            //prevent EF6 from inserting new parent
            _companyRepository.SetUnchanged(item.Company);
        }

        protected override async Task<Contract> CreateItemAsync(int parentId)
        {
            var item = dataRepository.Create();

            if (parentId <= 0) return await Task.FromResult(item);

            item.Company = await _companyRepository.GetAsync(parentId);
            item.CompanyId = parentId;

            return item;
        }

        protected override void RegisterIDisposables(List<IDisposable> disposables)
        {
            disposables.Add(_companyRepository);
        }
    }
}
