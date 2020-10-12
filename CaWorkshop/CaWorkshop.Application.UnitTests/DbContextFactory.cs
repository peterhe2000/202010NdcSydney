﻿using CaWorkshop.Infrastructure.Persistence;
using IdentityServer4.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;

namespace CaWorkshop.Application.UnitTests
{
    public static class DbContextFactory
    {
        public static ApplicationDbContext Create()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            // Identity in memory
            var operationalStoreOptions = Options.Create(
                new OperationalStoreOptions
                {
                    DeviceFlowCodes =
                        new TableConfiguration("DeviceCodes"),
                    PersistedGrants =
                        new TableConfiguration("PersistedGrants")
                });

            var context = new ApplicationDbContext(
                options, operationalStoreOptions);

            // seed database warning:
            // NOTE: this centralised data will likely cause issues at some point, 
            // when someone changes the data to suit the needs of another tests.
            ApplicationDbContextSeeder.Seed(context);

            return context;
        }

        public static void Destroy(ApplicationDbContext context)
        {
            context.Database.EnsureDeleted();
            context.Dispose();
        }
    }
}