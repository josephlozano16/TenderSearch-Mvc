using Eml.DataRepository;
using Eml.DataRepository.Extensions;
using System.Data.Entity.Migrations;
using TenderSearch.Business.Common.Entities;

namespace TenderSearch.DataMigration.Seeders
{
    public static class ContactPersonSeeder
    {
        public static void Seed<T>(T context)
            where T : Data.TenderSearchDb
        {
            Seeder.Execute("ContactPersons", () =>
            {
                context.ContactPersons.AddOrUpdate(r => r.Id,
                    new ContactPerson { Id = 1, CompanyId = 1, FirstName = "John", LastName = "Doe", ContractType = Contracts.Infrastructure.Contracts.Master, PositionTitleId = 1, Department = "IT Department", Email = "john.doe@tendersearch.com", IsActive = true, PhoneNumber = "+123456789" }
                );

                context.DoSave("ContactPersons");
            });
        }
    }
}
