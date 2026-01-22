using TourismPlatform.Data;
using TourismPlatform.Models;

namespace TourismPlatform.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<User> Users { get; }
        IRepository<AdminUser> AdminUsers { get; }
        IRepository<District> Districts { get; }
        IRepository<AttractionCategory> AttractionCategories { get; }
        IRepository<Attraction> Attractions { get; }
        IRepository<AttractionImage> AttractionImages { get; }
        IRepository<Food> Foods { get; }
        IRepository<Hotel> Hotels { get; }
        IRepository<HotelRoomType> HotelRoomTypes { get; }
        IRepository<TicketOrder> TicketOrders { get; }
        IRepository<HotelOrder> HotelOrders { get; }
        IRepository<VerifyCode> VerifyCodes { get; }
        IRepository<Comment> Comments { get; }
        IRepository<Rating> Ratings { get; }
        IRepository<TravelDiary> TravelDiaries { get; }
        IRepository<Favorite> Favorites { get; }
        IRepository<TravelRoute> TravelRoutes { get; }
        IRepository<Announcement> Announcements { get; }
        MyDbContext Context { get; }

        Task<int> SaveChangesAsync();
    }
}
