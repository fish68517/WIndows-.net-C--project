using TourismPlatform.Data;
using TourismPlatform.Models;

namespace TourismPlatform.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MyDbContext _context;
        private IRepository<User> _users;
        private IRepository<AdminUser> _adminUsers;
        private IRepository<District> _districts;
        private IRepository<AttractionCategory> _attractionCategories;
        private IRepository<Attraction> _attractions;
        private IRepository<AttractionImage> _attractionImages;
        private IRepository<Food> _foods;
        private IRepository<Hotel> _hotels;
        private IRepository<HotelRoomType> _hotelRoomTypes;
        private IRepository<TicketOrder> _ticketOrders;
        private IRepository<HotelOrder> _hotelOrders;
        private IRepository<VerifyCode> _verifyCodes;
        private IRepository<Comment> _comments;
        private IRepository<Rating> _ratings;
        private IRepository<TravelDiary> _travelDiaries;
        private IRepository<Favorite> _favorites;
        private IRepository<TravelRoute> _travelRoutes;
        private IRepository<Announcement> _announcements;

        public UnitOfWork(MyDbContext context)
        {
            _context = context;
        }

        public IRepository<User> Users => _users ??= new Repository<User>(_context);
        public IRepository<AdminUser> AdminUsers => _adminUsers ??= new Repository<AdminUser>(_context);
        public IRepository<District> Districts => _districts ??= new Repository<District>(_context);
        public IRepository<AttractionCategory> AttractionCategories => _attractionCategories ??= new Repository<AttractionCategory>(_context);
        public IRepository<Attraction> Attractions => _attractions ??= new Repository<Attraction>(_context);
        public IRepository<AttractionImage> AttractionImages => _attractionImages ??= new Repository<AttractionImage>(_context);
        public IRepository<Food> Foods => _foods ??= new Repository<Food>(_context);
        public IRepository<Hotel> Hotels => _hotels ??= new Repository<Hotel>(_context);
        public IRepository<HotelRoomType> HotelRoomTypes => _hotelRoomTypes ??= new Repository<HotelRoomType>(_context);
        public IRepository<TicketOrder> TicketOrders => _ticketOrders ??= new Repository<TicketOrder>(_context);
        public IRepository<HotelOrder> HotelOrders => _hotelOrders ??= new Repository<HotelOrder>(_context);
        public IRepository<VerifyCode> VerifyCodes => _verifyCodes ??= new Repository<VerifyCode>(_context);
        public IRepository<Comment> Comments => _comments ??= new Repository<Comment>(_context);
        public IRepository<Rating> Ratings => _ratings ??= new Repository<Rating>(_context);
        public IRepository<TravelDiary> TravelDiaries => _travelDiaries ??= new Repository<TravelDiary>(_context);
        public IRepository<Favorite> Favorites => _favorites ??= new Repository<Favorite>(_context);
        public IRepository<TravelRoute> TravelRoutes => _travelRoutes ??= new Repository<TravelRoute>(_context);
        public IRepository<Announcement> Announcements => _announcements ??= new Repository<Announcement>(_context);
        public MyDbContext Context => _context;

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
