using System.Collections.Generic;
using Eml.Asp.Identity;
using Eml.DataRepository;
using Eml.DataRepository.Extensions;
using TenderSearch.Contracts.Infrastructure;
using TenderSearch.Data.Security;

namespace TenderSearch.DataMigration.Seeders
{
    public static class UserSeeder
    {
        public static void Seed<T>(T context)
            where T : Data.TenderSearchDb
        {
            Seeder.Execute("Users", () =>
            {
                Seeder.Execute("Users", () =>
                {
                    const string EMAIL_DOMAIN = "tendersearch.com";

                    var emlIdentityMigration = context.CreateIdentityMigration<ApplicationUser>();
                    var hans = new UserWithRoles("Hans", new[] { UserRoles.Admins.ToString(), UserRoles.UserManagers.ToString(), UserRoles.Users.ToString() }, EMAIL_DOMAIN);
                    var kyle = new UserWithRoles("Kyle", new[] { UserRoles.UserManagers.ToString() }, EMAIL_DOMAIN);
                    var kitkat = new UserWithRoles("KitKat", new[] { UserRoles.Users.ToString() }, EMAIL_DOMAIN);
                    var withRoles = new List<UserWithRoles>(new[] { hans, kyle, kitkat });

                    emlIdentityMigration.CreateUsers(withRoles);
                    context.DoSave("Users");
                });
            });
        }
    }
}