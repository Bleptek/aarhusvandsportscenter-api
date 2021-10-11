using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace Aarhusvandsportscenter.Api.Infastructure.Database
{
    /// <summary>
    /// Used upon Updating database from local development using the EF cli, which you should basically not be doing. 
    /// You should let the app take care of applying migrations upon startup.
    /// The connection string here is not used when running the application and can as such be left with the dev/local credentials or even empty.
    /// If in doubt, read up on the official documentation of a DbContextFactory
    /// </summary>
    public class DbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseMySql("server=someserver.com;database=somedb;user=someuser;password=somepw",
                        new MySqlServerVersion(new Version(5, 7, 32)),
                        mySqlOptions => mySqlOptions
                            .CharSetBehavior(CharSetBehavior.NeverAppend));

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}