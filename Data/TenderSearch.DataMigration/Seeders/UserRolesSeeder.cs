using Eml.Asp.Identity;
using Eml.DataRepository;
using Eml.DataRepository.Extensions;
using TenderSearch.Contracts.Infrastructure;
using TenderSearch.Data.Security;

namespace TenderSearch.DataMigration.Seeders
{
    public static class UserRolesSeeder
    {
        public static void Seed<T>(T context)
            where T : Data.TenderSearchDb
        {
            Seeder.Execute("UserRoles", () =>
            {
                var emlIdentityMigration = context.CreateIdentityMigration<ApplicationUser>();
                emlIdentityMigration.CreateRoles(new[] { UserRoles.Admins.ToString(), UserRoles.UserManagers.ToString(), UserRoles.Users.ToString() });

                context.DoSave("UserRoles");
            });
        }
    }
}