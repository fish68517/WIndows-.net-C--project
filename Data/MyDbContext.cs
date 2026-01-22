using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TourismPlatform.Models;

namespace TourismPlatform.Data
{
    public class MyDbContext : IdentityDbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }

        // User and Account
        public DbSet<User> Users { get; set; }
        public DbSet<AdminUser> AdminUsers { get; set; }

        // Location and Category
        public DbSet<District> Districts { get; set; }
        public DbSet<AttractionCategory> AttractionCategories { get; set; }

        // Attractions
        public DbSet<Attraction> Attractions { get; set; }
        public DbSet<AttractionImage> AttractionImages { get; set; }

        // Food and Hotel
        public DbSet<Food> Foods { get; set; }
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<HotelRoomType> HotelRoomTypes { get; set; }

        // Orders
        public DbSet<TicketOrder> TicketOrders { get; set; }
        public DbSet<HotelOrder> HotelOrders { get; set; }
        public DbSet<VerifyCode> VerifyCodes { get; set; }

        // Community
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<TravelDiary> TravelDiaries { get; set; }
        public DbSet<Favorite> Favorites { get; set; }

        // Admin
        public DbSet<TravelRoute> TravelRoutes { get; set; }
        public DbSet<Announcement> Announcements { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure User relationships
            builder.Entity<User>()
                .HasMany(u => u.TravelDiaries)
                .WithOne(d => d.User)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<User>()
                .HasMany(u => u.Comments)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict); // <--- 已改为 Restrict (解决用户删除时的评论冲突)

            builder.Entity<User>()
                .HasMany(u => u.Favorites)
                .WithOne(f => f.User)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<User>()
                .HasMany(u => u.TicketOrders)
                .WithOne(o => o.User)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<User>()
                .HasMany(u => u.HotelOrders)
                .WithOne(o => o.User)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure District relationships
            builder.Entity<District>()
                .HasMany(d => d.Attractions)
                .WithOne(a => a.District)
                .HasForeignKey(a => a.DistrictId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<District>()
                .HasMany(d => d.Foods)
                .WithOne(f => f.District)
                .HasForeignKey(f => f.DistrictId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<District>()
                .HasMany(d => d.Hotels)
                .WithOne(h => h.District)
                .HasForeignKey(h => h.DistrictId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure AttractionCategory relationships
            builder.Entity<AttractionCategory>()
                .HasMany(c => c.Attractions)
                .WithOne(a => a.Category)
                .HasForeignKey(a => a.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Attraction relationships
            builder.Entity<Attraction>()
                .HasMany(a => a.Images)
                .WithOne(i => i.Attraction)
                .HasForeignKey(i => i.AttractionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Attraction>()
                .HasMany(a => a.Comments)
                .WithOne(c => c.Attraction)
                .HasForeignKey(c => c.AttractionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Attraction>()
                .HasMany(a => a.Favorites)
                .WithOne(f => f.Attraction)
                .HasForeignKey(f => f.AttractionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Attraction>()
                .HasMany(a => a.TicketOrders)
                .WithOne(o => o.Attraction)
                .HasForeignKey(o => o.AttractionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Food relationships
            builder.Entity<Food>()
                .HasOne(f => f.Attraction)
                .WithMany(a => a.Foods)
                .HasForeignKey(f => f.AttractionId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Food>()
                .HasOne(f => f.District)
                .WithMany(d => d.Foods)
                .HasForeignKey(f => f.DistrictId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Hotel relationships
            builder.Entity<Hotel>()
                .HasOne(h => h.Attraction)
                .WithMany(a => a.Hotels)
                .HasForeignKey(h => h.AttractionId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Hotel>()
                .HasOne(h => h.District)
                .WithMany(d => d.Hotels)
                .HasForeignKey(h => h.DistrictId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Hotel>()
                .HasMany(h => h.RoomTypes)
                .WithOne(r => r.Hotel)
                .HasForeignKey(r => r.HotelId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure HotelRoomType relationships
            builder.Entity<HotelRoomType>()
                .HasMany(r => r.Orders)
                .WithOne(o => o.RoomType)
                .HasForeignKey(o => o.RoomTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure TicketOrder relationships
            builder.Entity<TicketOrder>()
                .HasOne(o => o.VerifyCode)
                .WithOne(v => v.TicketOrder)
                .HasForeignKey<VerifyCode>(v => v.TicketOrderId)
                .OnDelete(DeleteBehavior.Restrict); // <--- 已改为 Restrict (解决订单删除时的验证码冲突)

            // Configure HotelOrder relationships
            builder.Entity<HotelOrder>()
                .HasOne(o => o.VerifyCode)
                .WithOne(v => v.HotelOrder)
                .HasForeignKey<VerifyCode>(v => v.HotelOrderId)
                .OnDelete(DeleteBehavior.Restrict); // <--- 已改为 Restrict (解决订单删除时的验证码冲突)

            // Configure Comment relationships
            builder.Entity<Comment>()
                .HasOne(c => c.Attraction)
                .WithMany(a => a.Comments)
                .HasForeignKey(c => c.AttractionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Comment>()
                .HasOne(c => c.Diary)
                .WithMany(d => d.Comments)
                .HasForeignKey(c => c.DiaryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Rating relationships
            builder.Entity<Rating>()
                .HasOne(r => r.Attraction)
                .WithMany()
                .HasForeignKey(r => r.AttractionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Rating>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure TravelDiary relationships
            builder.Entity<TravelDiary>()
                .HasMany(d => d.Comments)
                .WithOne(c => c.Diary)
                .HasForeignKey(c => c.DiaryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Favorite relationships
            builder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Favorite>()
                .HasOne(f => f.Attraction)
                .WithMany(a => a.Favorites)
                .HasForeignKey(f => f.AttractionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Add unique constraints
            builder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Configure Decimal Precision (修复金额字段警告)
            builder.Entity<Attraction>()
                .Property(a => a.TicketPrice)
                .HasPrecision(18, 2);

            builder.Entity<Food>()
                .Property(f => f.ReferencePrice)
                .HasPrecision(18, 2);

            builder.Entity<HotelRoomType>()
                .Property(r => r.PricePerNight)
                .HasPrecision(18, 2);

            builder.Entity<TicketOrder>()
                .Property(o => o.TotalPrice)
                .HasPrecision(18, 2);

            builder.Entity<HotelOrder>()
                .Property(o => o.TotalPrice)
                .HasPrecision(18, 2);

            // 其他 Unique 约束
            builder.Entity<VerifyCode>()
                .HasIndex(v => v.Code)
                .IsUnique();

            builder.Entity<Favorite>()
                .HasIndex(f => new { f.UserId, f.AttractionId })
                .IsUnique();
        }
    }
}