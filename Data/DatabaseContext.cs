using Microsoft.EntityFrameworkCore;
using CarRentalSystem_WPF.Models;

namespace CarRentalSystem_WPF.Data
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Car> Cars { get; set; }
        public DbSet<Renter> Renters { get; set; }
        public DbSet<Paperwork> Paperworks { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=car_rental_system.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Car entity
            modelBuilder.Entity<Car>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Brand).IsRequired();
                entity.Property(e => e.Model).IsRequired();
                entity.Property(e => e.LicensePlate).IsRequired();
            });

            // Configure Renter entity
            modelBuilder.Entity<Renter>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FullName).IsRequired();
                entity.Property(e => e.DriverLicenseNumber).IsRequired();
            });

            // Configure Paperwork entity
            modelBuilder.Entity<Paperwork>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Car)
                      .WithMany()
                      .HasForeignKey(e => e.CarId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Renter)
                      .WithMany()
                      .HasForeignKey(e => e.RenterId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                // Ignore display-only properties that shouldn't be stored in the database
                entity.Ignore(e => e.CarBrand);
                entity.Ignore(e => e.CarModel);
                entity.Ignore(e => e.CarLicensePlate);
                entity.Ignore(e => e.CarPricePerDay);
                entity.Ignore(e => e.RenterName);
                entity.Ignore(e => e.RenterPhoneNumber);
            });
        }
    }
}
