using System.Data.Entity.Migrations;

namespace TenderSearch.DataMigration.SqlServerMigrations
{
    public partial class SeedContactPersons : DbMigration
    {
        public override void Up()
        {
            const string TABLE_NAME = "ContactPersons";
            const string COLUMNS = "Id, CompanyId, PositionTitleId, FirstName, LastName, ContractType, Email, PhoneNumber, Department, IsActive";

            Sql($@"SET IDENTITY_INSERT {TABLE_NAME} ON; 
                INSERT INTO {TABLE_NAME} ({COLUMNS}) VALUES (1, 1, 1, 'John', 'Doe', 'Master contact', 'john.doe@tendersearch.com', '+123456789', 'IT Department', '1')
                SET IDENTITY_INSERT {TABLE_NAME} OFF");
        }

        public override void Down()
        {
            const string TABLE_NAME = "ContactPersons";
            const string ID_COLUMN = "Id";

            Sql($@"DELETE FROM {TABLE_NAME} WHERE {ID_COLUMN} IN (1)");
        }
    }
}
