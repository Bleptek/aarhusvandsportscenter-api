using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aarhusvandsportscenter.Api.Infastructure.Database
{
    public class AppDbContext : DbContext
    {

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public AppDbContext()
        {
        }

        public DbSet<ContentPageEntity> ContentPages { get; set; }
        public DbSet<ContentPageSectionEntity> ContentPageSections { get; set; }
        public DbSet<ContentPageImageEntity> ContentPageImages { get; set; }
        public DbSet<AccountEntity> Accounts { get; set; }

        public DbSet<RentalEntity> Rentals { get; set; }
        public DbSet<RentalItemEntity> RentalItems { get; set; }
        public DbSet<RentalCategoryEntity> RentalCategories { get; set; }
        public DbSet<RentalProductEntity> RentalProducts { get; set; }
        public DbSet<RentalProductPriceEntity> RentalProductPrices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ContentPageEntity>()
                .HasIndex(b => b.Key).IsUnique();

            modelBuilder.Entity<ContentPageSectionEntity>()
                .HasIndex(b => new { b.Key, b.ContentPageId }).IsUnique();

            modelBuilder.Entity<AccountEntity>()
                .HasIndex(b => b.Email).IsUnique();

            modelBuilder.Entity<RentalEntity>().HasIndex(b => b.StartDate);
            modelBuilder.Entity<RentalEntity>().HasIndex(b => b.EndDate);
            
            modelBuilder.Entity<RentalItemEntity>()
                .HasIndex(b => new { b.RentalId, b.ProductId }).IsUnique();

            modelBuilder.Entity<RentalCategoryEntity>()
                .HasIndex(x => x.Name).IsUnique();

            modelBuilder.Entity<RentalProductEntity>()
                .HasIndex(x => x.Name).IsUnique();

            modelBuilder.Entity<RentalProductPriceEntity>()
                .HasIndex(x => new { x.ProductId, x.Quantity }).IsUnique();
        }
    }
}