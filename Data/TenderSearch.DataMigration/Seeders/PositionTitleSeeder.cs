using Eml.DataRepository;
using Eml.DataRepository.Extensions;
using System.Data.Entity.Migrations;
using TenderSearch.Business.Common.Entities;

namespace TenderSearch.DataMigration.Seeders
{
    public static class PositionTitleSeeder
    {
        public static void Seed<T>(T context)
            where T : Data.TenderSearchDb
        {
            Seeder.Execute("PositionTitles", () =>
            {
                context.PositionTitles.AddOrUpdate(r => r.Id,
                    new PositionTitle { Id = 1, Title = "CEO", DateDeleted = null, DeletionReason = "" },
                    new PositionTitle { Id = 2, Title = "Manager", DateDeleted = null, DeletionReason = "" },
                    new PositionTitle { Id = 3, Title = "Vice President", DateDeleted = null, DeletionReason = "" }
                );

                context.DoSave("PositionTitles");
            });
        }
    }
}