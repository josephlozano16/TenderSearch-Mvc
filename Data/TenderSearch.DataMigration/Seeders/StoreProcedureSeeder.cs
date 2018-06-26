using System;
using System.IO;
using System.Linq;
using Eml.DataRepository;
using Eml.DataRepository.Extensions;

namespace TenderSearch.DataMigration.Seeders
{
    public static class StoreProcedureSeeder
    {
        public static void Seed<T>(T context)
            where T : Data.TenderSearchDb
        {
            Seeder.Execute("StoreProcedures", () =>
            {
                var baseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"SqlScripts");
                var sqlFilesForDrop = Directory.GetFiles(baseDirectory, "Drop*.sql").ToList();
                var sqlFilesForCreate = Directory.GetFiles(baseDirectory, "Create*.sql").ToList();

                sqlFilesForDrop.ForEach(fn =>
                {
                    context.Database.ExecuteSqlCommand(File.ReadAllText(fn), new object[0]);
                });

                sqlFilesForCreate.ForEach(fn =>
                {
                    context.Database.ExecuteSqlCommand(File.ReadAllText(fn), new object[0]);
                });

                context.DoSave("StoreProcedures");
            });
        }
    }
}