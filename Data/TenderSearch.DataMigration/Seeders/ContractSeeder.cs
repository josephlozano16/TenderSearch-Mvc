﻿using System;
using System.Data.Entity.Migrations;
using System.Globalization;
using Eml.DataRepository;
using Eml.DataRepository.Extensions;
using TenderSearch.Business.Common.Entities;

namespace TenderSearch.DataMigration.Seeders
{
    public static class ContractSeeder
    {
        public static void Seed<T>(T context)
            where T : Data.TenderSearchDb
        {
            Seeder.Execute("Contracts", () =>
            {
                context.Contracts.AddOrUpdate(r => r.Id,
                    new Contract
                    {
                        Id = 1,
                        CompanyId = 1,
                        DateSigned = DateTime.Parse("18/01/2017", new CultureInfo("en-AU", false)),
                        RenewalDate = DateTime.Parse("18/01/2017", new CultureInfo("en-AU", false)),
                        EndDate = DateTime.Parse("18/01/2018", new CultureInfo("en-AU", false)),
                        Price = 100,
                        ContractType = "Master contact"
                    },
                    new Contract
                    {
                        Id = 2,
                        CompanyId = 1,
                        DateSigned = DateTime.Parse("19/01/2018", new CultureInfo("en-AU", false)),
                        RenewalDate = DateTime.Parse("19/01/2018", new CultureInfo("en-AU", false)),
                        EndDate = DateTime.Parse("19/01/2019", new CultureInfo("en-AU", false)),
                        Price = 200,
                        ContractType = "Standard contact"
                    }
                );

                context.DoSave("Contracts");
            });
        }
    }
}