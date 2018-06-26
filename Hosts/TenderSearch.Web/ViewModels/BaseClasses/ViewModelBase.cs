using System;
using System.Collections.Generic;
using System.Web.Mvc;
using TenderSearch.Data;

namespace TenderSearch.Web.ViewModels.BaseClasses
{
    public abstract class ViewModelBase<T> : IDisposable
        where T : class
    {
        protected ViewModelBase(T item)
        {
            Item = item;
        }
        public T Item { get; set; }
        protected TenderSearchDb db => _db ?? (_db = new TenderSearchDb());
        private TenderSearchDb _db;
        public abstract bool HasParent();
        public abstract int? GetParentID();
        public virtual IEnumerable<SelectListItem> GetParents(int ParentId)
        {
            return null;
        }

        public void Dispose()
        {
            _db?.Dispose();
        }
    }
}