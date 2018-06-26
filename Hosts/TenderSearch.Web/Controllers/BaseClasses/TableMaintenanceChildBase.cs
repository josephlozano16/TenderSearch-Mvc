using System.Collections.Generic;
using Eml.DataRepository.Contracts;
using System.Threading.Tasks;
using Eml.Contracts.Entities;
using Eml.Mediator.Contracts;
using TenderSearch.Contracts.Dto;

namespace TenderSearch.Web.Controllers.BaseClasses
{
    public abstract class TableMaintenanceChildBase<T> : TableMaintenanceBase<T>
        where T : class, IEntityBase<int>, IEntitySoftdeletableBase, new()
    {
        protected TableMaintenanceChildBase(IMediator mediator, IDataRepositorySoftDeleteInt<T> dataRepository) 
            : base(mediator, dataRepository)
        {
        }

        #region DEFAULT OVERRIDES FROM PARENT
        protected abstract override string GetModelTitle(T item);

        protected abstract override Task<IEnumerable<string>> GetAutoCompleteIntellisenseAsync(int? parentId, string term);

        protected abstract override Task<object> GetPagedListAsync(int? parentId, string searchTerm = null, int page = 1);

        protected abstract override T FinalizeCreate(T item);

        protected abstract override Task<UiMessage> IsDuplicateAsync(T item, string actionName);

        protected abstract override string GetParentLabelForEdit(T item);

        protected abstract override string GetClassTitle();
        #endregion // DEFAULT OVERRIDES FROM PARENT

        #region REQUIRED OVERRIDE IF CHILD ENTITY
        protected abstract override  Task<string> GetParentLabelAsync(int parentId);

        protected abstract override void FinalizeEdit(T item);

        protected abstract override Task<T> CreateItemAsync(int parentId);
        #endregion // REQUIRED OVERRIDE IF CHILD ENTITY
    }
}