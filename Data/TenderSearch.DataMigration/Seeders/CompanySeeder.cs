using System.Data.Entity.Migrations;
using Eml.DataRepository;
using Eml.DataRepository.Extensions;
using TenderSearch.Business.Common.Entities;

namespace TenderSearch.DataMigration.Seeders
{
    public static class CompanySeeder
    {
        public static void Seed<T>(T context)
            where T : Data.TenderSearchDb
        {
            Seeder.Execute("Companies", () =>
            {
                context.Companies.AddOrUpdate(r => r.Id,
                    new Company { Id = 1, Name = "Company01", Description = "Description01", Website = "http://Website01.com", AbnCan = "AbnCan01" },
                    new Company { Id = 2, Name = "Company02", Description = "Description02", Website = "http://Website02.com", AbnCan = "AbnCan02" }
                );

                context.DoSave("Companies");
            });
        }
    }
}