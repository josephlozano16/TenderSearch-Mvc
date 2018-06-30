using System.Data.Entity.Migrations;

namespace TenderSearch.DataMigration
{
    public sealed class MigrationConfiguration : DbMigrationsConfiguration<TenderSearchDb>
    {
        ///// <summary>
        ///// Set this to false before the project's first production deployment
        ///// </summary>
        //private const bool USE_PRODUCTION_MIGRATION = false; 

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
            //if (!USE_PRODUCTION_MIGRATION)
            //{
            //    // This approach will be useful only during the early phase of development 
            //    // when lots of entity rafactoring is enevitable.
            //    CompanySeeder.Seed(context);
            //    ContractSeeder.Seed(context);
            //    PositionTitleSeeder.Seed(context);
            //    ContactPersonSeeder.Seed(context);
            //    UserRolesSeeder.Seed(context);
            //    UserSeeder.Seed(context);
            //    StoreProcedureSeeder.Seed(context);
            //}
        }
    }
}