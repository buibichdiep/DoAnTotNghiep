using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Booking.Areas.Admin.Models.Hotel;
using CloudinaryDotNet.Core;
using Booking.Areas.Admin.Models.Tour;
using Booking.Areas.Admin.Models.Flight;
using Booking.Areas.Admin.Models.Admin;
using Booking.Areas.Accounts.Models.Account;

namespace Booking.EF
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public override DbSet<User> Users { get; set; }
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<Media> Medias { get; set; }
        public DbSet<Utility> Utilities { get; set; }
        public DbSet<HotelUtility> HotelUtilities{ get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<CategoryTour> CategoryTours { get; set; }
        public DbSet<CategoryHotel> CategoryHotels { get; set; }
        public DbSet<Tour> Tours { get; set; }
        public DbSet<TravelSchedule> TravelSchedules { get; set; }
        public DbSet<CityCodeIATA> CityCodes { get; set; }
        public DbSet<Bill> Bills { get; set; }
        public DbSet<BillPayment> BillPayments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                if (tableName!.StartsWith("AspNet"))
                {
                    entityType.SetTableName(tableName.Substring(6));
                }
            }

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.Id).HasMaxLength(50);
                entity.Property(u => u.PasswordHash).HasMaxLength(250);
                entity.Property(u => u.SecurityStamp).HasMaxLength(250);
                entity.Property(u => u.ConcurrencyStamp).HasMaxLength(250);
                entity.Property(u => u.PhoneNumber).HasMaxLength(250);
                entity.Property(u => u.BirthDay).HasMaxLength(50);
                entity.Property(u => u.Gender).HasMaxLength(10);
                entity.Property(u => u.UserName).IsRequired();
                entity.Property(u => u.FullName).HasMaxLength(250);
                entity.Property(u => u.PhoneNumber).IsRequired(false);
                entity.Property(u => u.Email).HasMaxLength(250).IsRequired();
                entity.HasIndex(u => new { u.Email, u.PhoneNumber })
                    .HasDatabaseName("Index_Email_PhoneNumber")
                    .IsUnique();
            });

            modelBuilder.Entity<CategoryHotel>(entity =>
            {
                entity.Property(ct => ct.Name).HasMaxLength(250);
                entity.Property(ct => ct.Slug).HasMaxLength(250);
                entity.HasIndex(u => u.Name)
                    .HasDatabaseName("Index_Name")
                    .IsUnique();

                entity.HasOne(ct => ct.CateHotelParent)
                    .WithMany(ctp => ctp.CateHotelChildren)
                    .HasForeignKey(ct => ct.IdParent)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Hotel>(entity =>
            {
                entity.Property(h => h.HotelName).HasMaxLength(500);
                entity.Property(h => h.Avatar).HasMaxLength(500);
                entity.Property(h => h.Address).HasMaxLength(250);
                entity.Property(h => h.Tag).HasMaxLength(250);
                entity.Property(h => h.Slug).HasMaxLength(500);
                entity.HasIndex(h => h.Slug)
                    .HasDatabaseName("Index_Slug")
                    .IsUnique();

                entity.HasOne(t => t.CateHotel)
                    .WithMany(ct => ct.Hotels)
                    .HasForeignKey(t => t.IdCateHotel)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Media>(entity =>
            {
                entity.Property(m => m.PublicId).HasMaxLength(250);
                entity.Property(m => m.MediaType).HasMaxLength(20);
                entity.Property(m => m.AuthorType).HasMaxLength(50);
            });

            modelBuilder.Entity<Utility>(entity =>
            {
                entity.Property(u => u.Icon).HasMaxLength(250);
                entity.Property(u => u.UtilityName).HasMaxLength(250);
                entity.HasIndex(u => u.UtilityName)
                    .HasDatabaseName("Index_UtilityName")
                    .IsUnique();
            });

            modelBuilder.Entity<HotelUtility>(entity =>
            {
                entity.HasKey(hu => new { hu.IdHotel, hu.IdUtility });

                entity.HasOne(hu => hu.Hotel) // Liên kết với bảng 1
                    .WithMany(h => h.HotelUtilitys) // Liên kết với bảng nhiều
                    .HasForeignKey(hu => hu.IdHotel) // Thiết lập khóa ngoại của bảng nhiều
                    .OnDelete(DeleteBehavior.Cascade); // Xóa HotelUtility khi hotel bị xóa

                entity.HasOne(hu => hu.Utility)
                    .WithMany(u => u.HotelUtilitys)
                    .HasForeignKey(hu => hu.IdUtility)
                    .OnDelete(DeleteBehavior.Cascade);// Xóa HotelUtility khi utility bị xóa
            });

            modelBuilder.Entity<Room>(entity =>
            {
                entity.Property(r => r.RoomName).HasMaxLength(500);
                entity.Property(r => r.Avatar).HasMaxLength(500);
                entity.Property(r => r.Style).HasMaxLength(50);
                entity.Property(r => r.Price).HasColumnType("money");
                entity.Property(r => r.Discount).HasColumnType("money");
                entity.Property(r => r.AmountPeople).HasMaxLength(250);
                entity.Property(r => r.Direction).HasMaxLength(250);
                entity.Property(r => r.Bed).HasMaxLength(250);
                entity.Property(r => r.BedMore).HasMaxLength(250);

                entity.HasOne(r => r.Hotel)
                    .WithMany(h => h.Rooms)
                    .HasForeignKey(r => r.IdHotel)
                    .OnDelete(DeleteBehavior.Cascade); // Xóa room nếu hotel bị xóa
            });

            modelBuilder.Entity<CategoryTour>(entity =>
            {
                entity.Property(ct => ct.Name).HasMaxLength(250);
                entity.Property(ct => ct.Slug).HasMaxLength(250);
                entity.HasIndex(u => u.Name)
                    .HasDatabaseName("Index_Name")
                    .IsUnique();

                entity.HasOne(ct => ct.CateTourParent)
                    .WithMany(ctp => ctp.CateTourChildren)
                    .HasForeignKey(ct => ct.IdParent)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Tour>(entity =>
            {
                entity.Property(t => t.TourName).HasMaxLength(500);
                entity.Property(t => t.Avatar).HasMaxLength(500);
                entity.Property(t => t.Duration).HasMaxLength(50);
                entity.Property(t => t.Departure).HasMaxLength(50);
                entity.Property(t => t.Destination).HasMaxLength(50);
                entity.Property(t => t.Sightseeing).HasMaxLength(250);
                entity.Property(t => t.Vehicle).HasMaxLength(250);
                entity.Property(t => t.Tag).HasMaxLength(250);
                entity.Property(t => t.Price).HasColumnType("money");
                entity.Property(t => t.Discount).HasColumnType("money");
                entity.Property(t => t.Description).HasMaxLength(1000);
                entity.Property(t => t.Overview).HasMaxLength(1000);
                entity.Property(t => t.ServiceInclude);
                entity.Property(t => t.ServiceNotInclude);
                entity.Property(h => h.Slug).HasMaxLength(500);
                entity.HasIndex(h => h.Slug)
                    .HasDatabaseName("Index_Slug")
                    .IsUnique();

                entity.HasOne(t => t.CateTour)
                    .WithMany(ct => ct.Tours)
                    .HasForeignKey(t => t.IdCateTour)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<TravelSchedule>(entity =>
            {
                entity.Property(ts => ts.Title).HasMaxLength(250);

                entity.HasOne(ts => ts.Tour)
                    .WithMany(t => t.TravelSchedules)
                    .HasForeignKey(ts => ts.IdTour)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CityCodeIATA>(entity =>
            {
                entity.Property(c => c.Id).HasDefaultValueSql("NEWID()");
                entity.Property(c => c.CityName).HasMaxLength(50);
                entity.Property(c => c.CodeIATA).HasMaxLength(50);
                entity.Property(c => c.CreateAt).HasDefaultValueSql("(sysdatetime())");

                entity.HasIndex(c => new { c.CityName, c.CodeIATA })
                    .HasDatabaseName("Index_CityName_CodeIATA")
                    .IsUnique();

                entity.HasOne(c => c.CityCodeIATAParent)
                    .WithMany(cp => cp.CityCodeIATAChildren)
                    .HasForeignKey(c => c.IdParent)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Bill>(entity =>
            {
                entity.Property(b => b.UserName).HasMaxLength(250);
                entity.Property(b => b.PhoneNumber).HasMaxLength(250);
                entity.Property(b => b.Email).HasMaxLength(250);
                entity.Property(b => b.ContentRequest).HasMaxLength(500);
                entity.Property(b => b.ServiceType).HasMaxLength(100);
                entity.Property(b => b.ServiceName).HasMaxLength(500);
                entity.Property(b => b.Price).HasColumnType("money");
                entity.Property(b => b.Tax).HasColumnType("money");
                entity.Property(b => b.Discount).HasColumnType("money");
                entity.Property(b => b.TotalPrice).HasColumnType("money");
                entity.Property(b => b.StatusBill).HasMaxLength(100);
            });

            modelBuilder.Entity<BillPayment>(entity =>
            {
                entity.Property(bp => bp.NumberAccountPayment).HasMaxLength(100);
                entity.Property(bp => bp.NameAccountPayment).HasMaxLength(250);
                entity.HasOne(bp => bp.Bill)
                    .WithMany(bp => bp.BillPayments)
                    .HasForeignKey(bp => bp.BillId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
