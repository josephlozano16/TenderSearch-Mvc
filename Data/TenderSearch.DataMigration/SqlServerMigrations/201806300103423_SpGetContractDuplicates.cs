using TenderSearch.DataMigration.Seeders;
    using System.Data.Entity.Migrations;
    
namespace TenderSearch.DataMigration.SqlServerMigrations
{
    public partial class SpGetContractDuplicates : DbMigration
    {
        public override void Up()
        {
            var sql = StoreProcedureSeeder.GetCreateScriptForGetContractDuplicates();

            Sql(sql);
        }

        public override void Down()
        {
            var sql = StoreProcedureSeeder.GetDropScriptForGetContractDuplicates();

            Sql(sql);
        }
    }
}
