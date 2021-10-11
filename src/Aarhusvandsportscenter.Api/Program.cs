using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Collections.Generic;
using System;
using MediatR;
using Aarhusvandsportscenter.Api.Domain.Commands.Accounts;
using Aarhusvandsportscenter.Api.Infastructure.Database;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;
using Aarhusvandsportscenter.Api.Domain.Commands.RentalCategories;
using Microsoft.Extensions.Configuration;

namespace Aarhusvandsportscenter.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                await MigrateDb(scope);
                await SeedDb(scope);
                await EnsureDefaultAdminAccounts(scope);
                await EnsureDefaultRentalCategory(scope);
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.Local.json", optional: true);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });


        private static async Task MigrateDb(IServiceScope scope)
        {
            var dbContext = scope.ServiceProvider.GetService<AppDbContext>();
            await dbContext.Database.MigrateAsync();
        }

        private static async Task SeedDb(IServiceScope scope)
        {
            var dbContext = scope.ServiceProvider.GetService<AppDbContext>();
            if (!await dbContext.ContentPages.AnyAsync())
                await dbContext.ContentPages.AddRangeAsync(
                    new List<ContentPageEntity>(){
                        new ContentPageEntity("kite-intro", "Kite-intro"){
                            Sections = new List<ContentPageSectionEntity>(){ new ContentPageSectionEntity("intro", null, "Dette er et kursus for dig")}},
                        new ContentPageEntity("kitesurfing", "Kitesurfing"){
                            Sections = new List<ContentPageSectionEntity>(){ new ContentPageSectionEntity("intro", null, "Dette er et kursus for dig")}},
                        new ContentPageEntity("windsurfing", "Windsurfing"){
                            Sections = new List<ContentPageSectionEntity>(){ new ContentPageSectionEntity("intro", null, "Dette er et kursus for dig")}},
                        new ContentPageEntity("garnfiskeri", "Garnfiskeri"){
                            Sections = new List<ContentPageSectionEntity>(){ new ContentPageSectionEntity("intro", null, "Dette er et kursus for dig")}},
                        new ContentPageEntity("selvforsvar", "Selvforsvar"){
                            Sections = new List<ContentPageSectionEntity>(){ new ContentPageSectionEntity("intro", null, "Dette er et kursus for dig")}},
                        new ContentPageEntity("kajak", "Kajak"){
                            Sections = new List<ContentPageSectionEntity>(){ new ContentPageSectionEntity("intro", null, "Dette er et kursus for dig")}},
                        new ContentPageEntity("kano", "Kano"){
                            Sections = new List<ContentPageSectionEntity>(){ new ContentPageSectionEntity("intro", null, "Dette er et kursus for dig")}},
                        new ContentPageEntity("jolle", "Jolle"){
                            Sections = new List<ContentPageSectionEntity>(){ new ContentPageSectionEntity("intro", null, "Dette er et kursus for dig")}},
                        new ContentPageEntity("paddle", "Paddle / sup"){
                            Sections = new List<ContentPageSectionEntity>(){ new ContentPageSectionEntity("intro", null, "Dette er et kursus for dig")}},
                        new ContentPageEntity("trailer", "Trailer"){
                            Sections = new List<ContentPageSectionEntity>(){ new ContentPageSectionEntity("intro", null, "Dette er et kursus for dig")}},
                        new ContentPageEntity("baadtrailer", "Baadtrailer"){
                            Sections = new List<ContentPageSectionEntity>(){ new ContentPageSectionEntity("intro", null, "Dette er et kursus for dig")}},
                        new ContentPageEntity("sikkerhed", "Sikkerhed"){
                            Sections = new List<ContentPageSectionEntity>(){ new ContentPageSectionEntity("intro", null, "Dette er et kursus for dig")}},
                        new ContentPageEntity("booking", "Booking"){
                            Sections = new List<ContentPageSectionEntity>(){ new ContentPageSectionEntity("intro", null, "Dette er et kursus for dig")}},
                        new ContentPageEntity("forside", "Forside"){
                            Sections = new List<ContentPageSectionEntity>(){ 
                                new ContentPageSectionEntity("intro", "Velkommen", "Vi udlejer bla bla."),
                                new ContentPageSectionEntity("undervisning", "Undervisning", "Vi underviser bla bla"),
                                new ContentPageSectionEntity("udlejning", "Udlejning", "Vi udlejer bla bla"),
                                new ContentPageSectionEntity("events", "Events", "Vi afholder events bla bla")}},
                        new ContentPageEntity("om-os", "Om os"){
                            Sections = new List<ContentPageSectionEntity>(){ new ContentPageSectionEntity("intro", null, "Dette er et kursus for dig")}},
                    }
                );

            if (!await dbContext.RentalProducts.AnyAsync())
                await dbContext.RentalProducts.AddRangeAsync(
                    new List<RentalProductEntity>(){
                        new RentalProductEntity("kajak", "kajakker", 12){Prices = new List<RentalProductPriceEntity>(){new RentalProductPriceEntity(1, 50)}},
                        new RentalProductEntity("kano", "kanoer", 4){Prices = new List<RentalProductPriceEntity>(){new RentalProductPriceEntity(1, 50)}}
                    }
                );

            await dbContext.SaveChangesAsync();
        }

        private static async Task EnsureDefaultAdminAccounts(IServiceScope scope)
        {
            var appsettings = scope.ServiceProvider.GetService<IOptions<Appsettings>>().Value;
            if(appsettings.CreateDefaultAdminAccounts == false)
                return;

            var dbContext = scope.ServiceProvider.GetService<AppDbContext>();
            var mediator = scope.ServiceProvider.GetService<IMediator>();
            var existingAccounts = await dbContext.Accounts.Select(x => x.Email).ToListAsync();

            foreach (var defaultAcc in appsettings.DefaultAdminAccounts)
            {
                if (existingAccounts.Contains(defaultAcc.Email, StringComparer.InvariantCultureIgnoreCase))
                    continue;

                await mediator.Send(new CreateAccount.Command(defaultAcc.FullName, defaultAcc.Email));
            }
        }

        private static async Task EnsureDefaultRentalCategory(IServiceScope scope)
        {
            var dbContext = scope.ServiceProvider.GetService<AppDbContext>();
            var mediator = scope.ServiceProvider.GetService<IMediator>();
            var defaultCategoryName = scope.ServiceProvider.GetService<IOptions<Appsettings>>().Value.Rental.DefaultCategoryName;
            var existingCategories = await dbContext.RentalCategories.Select(x => x.Name).ToListAsync();

            if (existingCategories.Contains(defaultCategoryName, StringComparer.InvariantCultureIgnoreCase))
                return;

            await mediator.Send(new CreateRentalCategory.Command(defaultCategoryName, "red", true));
        }
    }
}