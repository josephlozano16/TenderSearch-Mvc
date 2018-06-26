using System.Data.Entity.Migrations;

namespace TenderSearch.DataMigration.SqlServerMigrations
{
    public partial class SeedCompanies : DbMigration
    {
        public override void Up()
        {
            const string TABLE_NAME = "Companies";
            const string COLUMNS = "Id, Name, Description, Website, AbnCan";

            Sql($@"SET IDENTITY_INSERT {TABLE_NAME} ON; 
                INSERT INTO {TABLE_NAME} ({COLUMNS}) VALUES (1, 'Company01', 'Description01', 'http://Website01.com', 'AbnCan01')
                INSERT INTO {TABLE_NAME} ({COLUMNS}) VALUES (2, 'Company02', 'Description02', 'http://Website02.com', 'AbnCan02')
                SET IDENTITY_INSERT {TABLE_NAME} OFF");
        }

        public override void Down()
        {
            const string TABLE_NAME = "Companies";
            const string ID_COLUMN = "Id";

            Sql($@"DELETE FROM {TABLE_NAME} WHERE {ID_COLUMN} IN (1, 2)");
        }
    }
}
