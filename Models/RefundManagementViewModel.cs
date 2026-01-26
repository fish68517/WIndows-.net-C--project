using System.Collections.Generic;

namespace TourismPlatform.Models
{
    public class RefundManagementViewModel
    {
        public List<TicketOrder> TicketOrders { get; set; } = new List<TicketOrder>();
        public List<HotelOrder> HotelOrders { get; set; } = new List<HotelOrder>();
    }
}