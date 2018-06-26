using System.Data.Entity.Migrations;

namespace TenderSearch.DataMigration.SqlServerMigrations
{

    public partial class SeedContracts : DbMigration
    {
        public override void Up()
        {
            const string TABLE_NAME = "Contracts";
            const string COLUMNS = "Id, CompanyId, ContractType, Price, DateSigned, EndDate, RenewalDate";

            Sql($@"SET IDENTITY_INSERT {TABLE_NAME} ON; 
                INSERT INTO {TABLE_NAME} ({COLUMNS}) VALUES (1, 1, 'Master contact', 100, '2017-01-18', '2018-01-18', '2017-01-18')
                INSERT INTO {TABLE_NAME} ({COLUMNS}) VALUES (2, 1, 'Standard contact', 200, '2018-01-19', '2019-01-19', '2018-01-19')
                SET IDENTITY_INSERT {TABLE_NAME} OFF");
        }

        public override void Down()
        {
            const string TABLE_NAME = "Contracts";
            const string ID_COLUMN = "Id";

            Sql($@"DELETE FROM {TABLE_NAME} WHERE {ID_COLUMN} IN (1, 2)");
        }
    }
}
