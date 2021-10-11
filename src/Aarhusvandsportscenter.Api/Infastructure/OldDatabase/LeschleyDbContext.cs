using Aarhusvandsportscenter.Api.Infastructure.OldDatabase.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aarhusvandsportscenter.Api.Infastructure.OldDatabase
{
    public class LeschleyDbContext : DbContext
    {

        public LeschleyDbContext(DbContextOptions<LeschleyDbContext> options)
            : base(options)
        {
        }

        public LeschleyDbContext()
        {
        }

        public DbSet<BookingLejer> Booking_Lejere { get; set; }
    }
}