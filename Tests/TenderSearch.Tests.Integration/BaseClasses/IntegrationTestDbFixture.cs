﻿using System;
using System.Data;
using Eml.ClassFactory.Contracts;
using Eml.DataRepository.Attributes;
using Eml.DataRepository.Extensions;
using Eml.Mef;
using Xunit;

namespace TenderSearch.Tests.Integration.BaseClasses
{
    public class IntegrationTestDbFixture : IDisposable
    {
        public const string COLLECTION_DEFINITION = "TestDbNetFull CollectionDefinition";

        private const string DB_DIRECTORY = "DataBase";

        public static IClassFactory ClassFactory { get; private set; }

        private readonly IMigrator dbMigration;

        public IntegrationTestDbFixture()
        {
            Console.WriteLine("Bootstrapper.Init()..");
            ClassFactory = Bootstrapper.Init("TenderSearch*.dll");

            dbMigration = ClassFactory.GetMigrator(Environments.PRODUCTION);

            if (dbMigration == null)
            {
                throw new NoNullAllowedException("dbMigration not found..");
            }

            Console.WriteLine("CreateDb..");
            dbMigration.CreateDb(DB_DIRECTORY);
        }

        public void Dispose()
        {
            Console.WriteLine("DisposeDatabase..");

            dbMigration.DestroyDb();
            Eml.Mef.ClassFactory.Dispose(ClassFactory);
        }
    }

    [CollectionDefinition(IntegrationTestDbFixture.COLLECTION_DEFINITION)]
    public class IntegrationTestDbFixtureCollectionDefinition : ICollectionFixture<IntegrationTestDbFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
