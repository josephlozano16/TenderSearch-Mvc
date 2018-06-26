using System.Data.Entity.Migrations;

namespace TenderSearch.DataMigration.SqlServerMigrations
{
    public partial class SeedAspNetRoles : DbMigration
    {
        public override void Up()
        {
            const string TABLE_NAME = "AspNetRoles";
            const string COLUMNS = "Id, Name";

            Sql($@"INSERT INTO {TABLE_NAME} ({COLUMNS}) VALUES ('ce066b63-deab-4da6-bbd9-9384cd022d18', 'Admins')
                INSERT INTO {TABLE_NAME} ({COLUMNS}) VALUES ('3aec2eef-6ae3-4162-9113-c3d850d311dc', 'UserManagers')
                INSERT INTO {TABLE_NAME} ({COLUMNS}) VALUES ('385af7e8-b598-4ed7-a61c-a7d241707909', 'Users')");
        }

        public override void Down()
        {
            const string TABLE_NAME = "AspNetRoles";
            const string ID_COLUMN = "Id";

            Sql($@"DELETE FROM {TABLE_NAME} WHERE {ID_COLUMN} IN ('ce066b63-deab-4da6-bbd9-9384cd022d18', '3aec2eef-6ae3-4162-9113-c3d850d311dc', '385af7e8-b598-4ed7-a61c-a7d241707909')");
        }
    }
}
