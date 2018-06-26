using System.Data.Entity;
using TenderSearch.Business.Common.Entities;

namespace TenderSearch.Data
{
    public class TenderSearchDb : ApplicationDbContext
    {
        public DbSet<Company> Companies { get; set; }

        public DbSet<Contract> Contracts { get; set; }

        public DbSet<ContactPerson> ContactPersons { get; set; }

        public DbSet<PositionTitle> PositionTitles { get; set; }

        //protected override void OnModelCreating(DbModelBuilder modelBuilder)
        //{
        //    //var softDeleteColumnConvention = new AttributeToTableAnnotationConvention<SoftDeleteAttribute, string>(
        //    //    SoftDeleteColumn.Key, (type, attributes) => attributes.Single().SoftDeleteColumnName);

        //    //modelBuilder.Conventions.Add(softDeleteColumnConvention);

        //    base.OnModelCreating(modelBuilder);
        //}
    }
}