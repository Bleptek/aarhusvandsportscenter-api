using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MediatR;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Aarhusvandsportscenter.Api.Domain.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SendGrid;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;
using Aarhusvandsportscenter.Api.Infastructure.Database;
using Aarhusvandsportscenter.Api.Infastructure.Authorization;
using Aarhusvandsportscenter.Api.Infastructure;
using Aarhusvandsportscenter.Api.Infastructure.Middleware;
using Aarhusvandsportscenter.Api.Infastructure.OldDatabase;

namespace Aarhusvandsportscenter.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<Appsettings>(Configuration);
            var appsettings = Configuration.Get<Appsettings>();
            services.Configure<SendGridSettings>(Configuration.GetSection(nameof(Appsettings.SendGrid)));
            services.Configure<AuthorizationSettings>(Configuration.GetSection(nameof(Appsettings.Authorization)));
            services.Configure<RentalSettings>(Configuration.GetSection(nameof(Appsettings.Rental)));
            services.AddDbContext<AppDbContext>(opts =>
            {
                opts.UseMySql(Configuration.GetConnectionString("DbConnection"),
                        new MySqlServerVersion(new Version(5, 7, 32)), // found in phpmyadmin by executing SELECT VERSION();
                        mySqlOptions => mySqlOptions
                            .CharSetBehavior(CharSetBehavior.NeverAppend))
                    // Everything from this point on is optional but helps with debugging.
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors();
            });
            services.AddDbContext<LeschleyDbContext>(opts =>
            {
                opts.UseMySql(Configuration.GetConnectionString("LeschleyDbConnection"),
                    new MariaDbServerVersion(new Version(10, 4, 20)), // found in phpmyadmin by executing SELECT VERSION();
                    mySqlOptions => mySqlOptions
                        .CharSetBehavior(CharSetBehavior.NeverAppend))
                    // Everything from this point on is optional but helps with debugging.
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors();
            });
            
            services.AddScoped<IPasswordService, PasswordService>();
            services.AddScoped<IMailService, SendGridService>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<ISendGridClient, SendGridClient>(serviceProvider =>
            {
                return new SendGridClient(appsettings.SendGrid.ApiKey);
            });

            services.AddScoped<IJwtTokenHelper, JwtTokenHelper>();

            services.AddMediatR(typeof(Aarhusvandsportscenter.Api.Infastructure.Database.AppDbContext).Assembly); // use any type from Aarhusvandsportscenter.Api

            services.AddControllers()
                .AddJsonOptions(opt =>
                {
                    opt.JsonSerializerOptions.Converters.Add(new DateTimeUtcConverter());
                    opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                })
                .ConfigureApiBehaviorOptions();

            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "frontend/build";
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

            // SWAGGER
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Aarhus Vandsportscenter API",
                    Version = "v1"
                });

                var securitySchema = new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Bearer {token}",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "bearer",
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                };

                c.AddSecurityDefinition("Bearer", securitySchema);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        securitySchema,
                        new string[] {"Bearer"}
                    }
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath, true);
            });

        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            app.UseStaticFiles();
            app.UseSpaStaticFiles();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Arhus Vandsportscenter V1 API");
                c.RoutePrefix = "swagger";
            });


            if (env.IsDevelopment())
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
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "frontend";
            });
        }
    }
}
