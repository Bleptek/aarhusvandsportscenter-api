using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Aarhusvandsportscenter.Api.Domain.Commands.Accounts;
using Aarhusvandsportscenter.Api.Domain.Commands.RentalCategories;
using Aarhusvandsportscenter.Api.Domain.Services;
using Aarhusvandsportscenter.Api.Infastructure;
using Aarhusvandsportscenter.Api.Infastructure.Authorization;
using Aarhusvandsportscenter.Api.Infastructure.Database;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;
using Aarhusvandsportscenter.Api.Infastructure.Middleware;
using Aarhusvandsportscenter.Api.Infastructure.OldDatabase;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

namespace Aarhusvandsportscenter.Api
{
    public partial class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true);

            ConfigureServices(builder);

            var app = builder.Build();

            ConfigurePipeline(app);

            using (var scope = app.Services.CreateScope())
            {
                await MigrateDb(scope);
                // await SeedDb(scope);
                // await EnsureDefaultAdminAccounts(scope);
                // await EnsureDefaultRentalCategory(scope);
            }

            app.Run();
        }

        private static void ConfigureServices(WebApplicationBuilder builder)
        {
            var services = builder.Services;
            var configuration = builder.Configuration;

            services.Configure<Appsettings>(configuration);
            var appsettings = configuration.Get<Appsettings>();
            services.Configure<SendGridSettings>(configuration.GetSection(nameof(Appsettings.SendGrid)));
            services.Configure<AuthorizationSettings>(configuration.GetSection(nameof(Appsettings.Authorization)));
            services.Configure<RentalSettings>(configuration.GetSection(nameof(Appsettings.Rental)));
            services.Configure<SimplySmtpSettings>(configuration.GetSection(nameof(Appsettings.SimplySmtp)));
            services.AddDbContext<AppDbContext>(opts =>
            {
                opts.UseMySql(configuration.GetConnectionString("DbConnection"),
                        new MySqlServerVersion(new Version(5, 7, 32))) // found in phpmyadmin by executing SELECT VERSION();
                    // Everything from this point on is optional but helps with debugging.
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors();
            });
            services.AddDbContext<LeschleyDbContext>(opts =>
            {
                opts.UseMySql(configuration.GetConnectionString("LeschleyDbConnection"),
                    new MariaDbServerVersion(new Version(10, 4, 20))) // found in phpmyadmin by executing SELECT VERSION();
                    // Everything from this point on is optional but helps with debugging.
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors();
            });

            services.AddScoped<IPasswordService, PasswordService>();
            services.AddScoped<IMailService, SmtpMailService>();
            services.AddHttpContextAccessor();
            services.AddScoped<IJwtTokenHelper, JwtTokenHelper>();

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<AppDbContext>());

            services.AddControllers()
                .AddJsonOptions(opt =>
                {
                    opt.JsonSerializerOptions.Converters.Add(new DateTimeUtcConverter());
                    opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                })
                .ConfigureApiBehaviorOptions();

            services.AddSpaStaticFiles(spaConfiguration =>
            {
                spaConfiguration.RootPath = "frontend/build";
            });

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(cfg =>
                {
                    cfg.RequireHttpsMetadata = false;
                    cfg.SaveToken = true;
                    cfg.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = appsettings.Authorization.Issuer,
                        ValidateAudience = true,
                        ValidAudience = appsettings.Authorization.Audience,
                        ValidateLifetime = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appsettings.Authorization.JwtKey)),
                        ClockSkew = TimeSpan.Zero
                    };
                });
            services.AddAuthorization();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Aarhus Vandsportscenter API",
                    Version = "v1"
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme.",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });
                c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath, true);
            });
        }

        private static void ConfigurePipeline(WebApplication app)
        {
            app.UseStaticFiles();
            app.UseSpaStaticFiles();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Arhus Vandsportscenter V1 API");
                c.RoutePrefix = "swagger";
            });

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseMiddleware<ExceptionMiddleware>();

            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "frontend";
            });
        }

        private static async Task MigrateDb(IServiceScope scope)
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            // in-memory providers (used in tests) don't support migrations
            if (dbContext.Database.IsRelational())
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