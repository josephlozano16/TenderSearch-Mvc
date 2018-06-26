using Eml.DataRepository.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using Eml.Mediator.Contracts;
using TenderSearch.Business.Common.Entities;
using TenderSearch.Contracts.Dto;
using TenderSearch.Contracts.Infrastructure;
using TenderSearch.Web.Controllers.BaseClasses;
using X.PagedList;

namespace TenderSearch.Web.Areas.Users.Controllers
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [Authorize(Roles = Authorize.Users)]
    public class ContactPersonController : TableMaintenanceChildBase<ContactPerson>
    {
        private readonly IDataRepositorySoftDeleteInt<Company> _companyRepository;

        [ImportingConstructor]
        public ContactPersonController(IMediator mediator, IDataRepositorySoftDeleteInt<ContactPerson> dataRepository, IDataRepositorySoftDeleteInt<Company> companyRepository)
            : base(mediator, dataRepository)
        {
            _companyRepository = companyRepository;
        }

        protected override string GetModelTitle(ContactPerson item)
        {
            return item.Company.Name;
        }

        protected override async Task<IEnumerable<string>> GetAutoCompleteIntellisenseAsync(int? parentId, string term)
        {
            Expression<Func<ContactPerson, bool>> whereClause = r => r.FirstName.Contains(term)
                                                                     || r.LastName.Contains(term)
                                                                     || r.Company.Name.Contains(term)
                                                                     || r.PositionTitle.Title.Contains(term)
                                                                     || r.ContractType.Contains(term);
            if (parentId.HasValue)
            {
                whereClause = r => r.CompanyId == parentId 
                                   && (r.FirstName.Contains(term)
                                       || r.LastName.Contains(term)
                                       || r.Company.Name.Contains(term)
                                       || r.PositionTitle.Title.Contains(term)
                                       || r.ContractType.Contains(term));
            }

            return await dataRepository.GetAutoCompleteIntellisenseAsync(r => r.Include(s => s.Company).Include(s => s.PositionTitle), 
                whereClause, 
                r => r.OrderBy(s => s.FirstName), 
                r => r.FirstName);
        }

        protected override async Task<object> GetPagedListAsync(int? parentId, string searchTerm = null, int page = 1)
        {
            IEnumerable<ContactPerson> models;
            if (parentId.HasValue)
            {
                models = await dataRepository
                    .GetPagedListAsync(page
                        , r => r.Include(s => s.Company).Include(s => s.PositionTitle)
                        , r => r.CompanyId == parentId
                               && (searchTerm == null || r.FirstName.Contains(searchTerm) || r.LastName.Contains(searchTerm))
                        , r => r.OrderBy(s => s.ContractType));
            }
            else
            {
                models = await dataRepository
                    .GetPagedListAsync(page
                                        , r => r.Include(s => s.Company).Include(s => s.PositionTitle)
                                        , r => searchTerm == null || r.FirstName.Contains(searchTerm) || r.LastName.Contains(searchTerm)
                                        , r => r.OrderBy(s => s.ContractType));
            }

            return models.ToPagedList(page, PAGE_SIZE);
        }

        protected override ContactPerson FinalizeCreate(ContactPerson item)
        {
            item.Company = null; //prevent EF6 from inserting new parent

            return item;
        }

        protected override async Task<UiMessage> IsDuplicateAsync(ContactPerson item, string actionName)
        {
            var parentId = item.CompanyId;
            bool hasDuplicates;

            if (actionName == DuplicateNameAction.Create)
                hasDuplicates = await dataRepository
                    .HasDuplicatesAsync(r => r.FirstName.ToLower() == item.FirstName.ToLower()
                                             && r.LastName.ToLower() == item.LastName.ToLower()
                                             && r.Email.ToLower() == item.Email.ToLower()
                                             && r.CompanyId == parentId
                    );
            else
                hasDuplicates = await dataRepository
                    .HasDuplicatesAsync(r => r.FirstName.ToLower() == item.FirstName.ToLower()
                                             && r.LastName.ToLower() == item.LastName.ToLower()
                                             && r.Email.ToLower() == item.Email.ToLower()
                                             && r.CompanyId == parentId
                                             && r.Id != item.Id
                    );

            if (!hasDuplicates) return await Task.FromResult(new UiMessage());

            var cDuplicateMsg = $"Name: <strong>{item.FirstName} {item.LastName}</strong>";

            return await Task.FromResult(new UiMessage(new[] { cDuplicateMsg }));
        }

        protected override string GetParentLabelForEdit(ContactPerson item)
        {
            return item.Company.Name;
        }

        protected override string GetClassTitle()
        {
            return "Setup Contact Persons";
        }


        protected override async Task<string> GetParentLabelAsync(int parentId)
        {
            return await _companyRepository.GetValueAsync(x => x.Id == parentId, x => x.Name);
        }

        protected override void FinalizeEdit(ContactPerson item)
        {
            //prevent EF6 from inserting new parent
            _companyRepository.SetUnchanged(item.Company);
        }

        protected override async Task<ContactPerson> CreateItemAsync(int parentId)
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
