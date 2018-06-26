using System.Data.Entity.Migrations;

namespace TenderSearch.DataMigration.SqlServerMigrations
{
    public partial class SeedPositionTitles : DbMigration
    {
        public override void Up()
        {
            const string TABLE_NAME = "PositionTitles";
            const string COLUMNS = "Id, Title";

            Sql($@"SET IDENTITY_INSERT {TABLE_NAME} ON; 
                INSERT INTO {TABLE_NAME} ({COLUMNS}) VALUES (1, 'CEO')
                INSERT INTO {TABLE_NAME} ({COLUMNS}) VALUES (2, 'Manager')
                INSERT INTO {TABLE_NAME} ({COLUMNS}) VALUES (3, 'Vice President')
                SET IDENTITY_INSERT {TABLE_NAME} OFF");
        }

        public override void Down()
        {
            const string TABLE_NAME = "PositionTitles";
            const string ID_COLUMN = "Id";

            Sql($@"DELETE FROM {TABLE_NAME} WHERE {ID_COLUMN} IN (1, 2, 3)");
        }
    }
}
