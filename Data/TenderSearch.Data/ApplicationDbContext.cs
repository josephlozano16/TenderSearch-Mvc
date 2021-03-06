﻿using Microsoft.AspNet.Identity.EntityFramework;
using TenderSearch.Contracts.Infrastructure;
using TenderSearch.Data.Security;

namespace TenderSearch.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base(ConnectionStrings.TenderSearchKey, throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}
