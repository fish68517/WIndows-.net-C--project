using System;
using System.Collections.Generic;

namespace TourismPlatform.Models
{
    // 这里的类原先定义在 AdminOrdersController.cs 底部
    // 现在移动到这里，方便统一引用

    public class AdminOrderListViewModel
    {
        public string OrderType { get; set; } = "ticket";
        public string SelectedStatus { get; set; }
        public List<AdminTicketOrderViewModel> TicketOrders { get; set; } = new();
        public List<AdminHotelOrderViewModel> HotelOrders { get; set; } = new();
    }

       public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalAttractions { get; set; }
        public int TotalDiaries { get; set; }
        public int TotalTicketOrders { get; set; }
        public int TotalHotelOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingTicketOrders { get; set; }
        public int PendingHotelOrders { get; set; }
        public string AdminUsername { get; set; }
    }

    public class AdminTicketOrderViewModel
    {
        public int TicketOrderId { get; set; }
        public string UserEmail { get; set; }
        public string UserNickname { get; set; }
        public string AttractionName { get; set; }
        public DateTime VisitDate { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
    }

    public class AdminHotelOrderViewModel
    {
        public int HotelOrderId { get; set; }
        public string UserEmail { get; set; }
        public string UserNickname { get; set; }
        public string HotelName { get; set; }
        public string RoomTypeName { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
    }

    public class AdminTicketOrderDetailViewModel
    {
        public int TicketOrderId { get; set; }
        public string UserEmail { get; set; }
        public string UserNickname { get; set; }
        public string AttractionName { get; set; }
        public DateTime VisitDate { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public string VerifyCode { get; set; }
    }

    public class AdminHotelOrderDetailViewModel
    {
        public int HotelOrderId { get; set; }
        public string UserEmail { get; set; }
        public string UserNickname { get; set; }
        public string HotelName { get; set; }
        public string RoomTypeName { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public string VerifyCode { get; set; }
    }

    // ... 这里是之前添加的 AdminOrderListViewModel 等类 ...

    public class AdminLoginViewModel
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "邮箱不能为空")]
        [System.ComponentModel.DataAnnotations.EmailAddress(ErrorMessage = "邮箱格式不正确")]
        public string Email { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "密码不能为空")]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }


      // View Models
    public class AdminVerifyListViewModel
    {
        public string OrderType { get; set; } = "ticket";
        public List<AdminVerifyCodeViewModel> VerifyCodes { get; set; } = new();

    }

    public class VerificationViewModel
    {
       
           public List<TicketOrder> TicketOrders { get; set; } = new List<TicketOrder>();
        public List<HotelOrder> HotelOrders { get; set; } = new List<HotelOrder>();
    }

    public class AdminVerifyCodeViewModel
    {
        public int VerifyCodeId { get; set; }
        public string Code { get; set; }
        public int OrderId { get; set; }
        public string OrderType { get; set; }
        public string UserNickname { get; set; }
        public string UserEmail { get; set; }
        public string ItemName { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AdminVerifyDetailViewModel
    {
        public int VerifyCodeId { get; set; }
        public string Code { get; set; }
        public string OrderType { get; set; }
        public string UserNickname { get; set; }
        public string UserEmail { get; set; }
        public string ItemName { get; set; }
        public DateTime? VisitDate { get; set; }
        public int? Quantity { get; set; }
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string OrderStatus { get; set; }
        public string VerifyStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UsedAt { get; set; }
    }
}