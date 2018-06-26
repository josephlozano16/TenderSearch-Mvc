using System.Data.Entity.Migrations;
using TenderSearch.DataMigration.Seeders;

namespace TenderSearch.DataMigration
{
    public sealed class MigrationConfiguration : DbMigrationsConfiguration<TenderSearchDb>
    {
        public MigrationConfiguration()
        {
            var isEnabled = false; //Disable if running in Release Mode
#if DEBUG
            isEnabled = true;
#endif
            AutomaticMigrationsEnabled = isEnabled;
            AutomaticMigrationDataLossAllowed = isEnabled;

            MigrationsDirectory = "SqlServerMigrations";
            MigrationsNamespace = "TenderSearch.DataMigration.SqlServerMigrations";
        }

        protected override void Seed(TenderSearchDb context)
        {
            StoreProcedureSeeder.Seed(context);
        }
    }
}