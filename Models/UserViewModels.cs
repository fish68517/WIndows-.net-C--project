using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TourismPlatform.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "邮箱不能为空")]
        [EmailAddress(ErrorMessage = "邮箱格式不正确")]
        public string Email { get; set; }

        [Required(ErrorMessage = "密码不能为空")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "密码长度必须在8-100个字符之间")]
        public string Password { get; set; }

        [Required(ErrorMessage = "确认密码不能为空")]
        [Compare("Password", ErrorMessage = "两次输入的密码不一致")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "昵称不能为空")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "昵称长度必须在2-50个字符之间")]
        public string Nickname { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "邮箱不能为空")]
        [EmailAddress(ErrorMessage = "邮箱格式不正确")]
        public string Email { get; set; }

        [Required(ErrorMessage = "密码不能为空")]
        public string Password { get; set; }
    }

    public class CreateCommentViewModel
    {
        public int AttractionId { get; set; }

        [Required(ErrorMessage = "评论内容不能为空")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "评论内容长度必须在1-500字符之间")]
        public string Content { get; set; }

        [Required(ErrorMessage = "评分不能为空")]
        [Range(1, 5, ErrorMessage = "评分必须在1-5星之间")]
        public int Rating { get; set; }
    }

    public class CreateDiaryViewModel
    {
        [Required(ErrorMessage = "标题不能为空")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "标题长度必须在1-200字符之间")]
        public string Title { get; set; }

        [Required(ErrorMessage = "内容不能为空")]
        [StringLength(5000, MinimumLength = 1, ErrorMessage = "内容长度必须在1-5000字符之间")]
        public string Content { get; set; }

        public IFormFileCollection Images { get; set; }
    }

    public class PersonalDashboardViewModel
    {
        public User User { get; set; }
        public int TicketOrderCount { get; set; }
        public int HotelOrderCount { get; set; }
        public int DiaryCount { get; set; }
        public int FavoriteCount { get; set; }
        public List<TicketOrder> RecentTicketOrders { get; set; }
        public List<HotelOrder> RecentHotelOrders { get; set; }
        public List<TravelDiary> RecentDiaries { get; set; }
    }

    public class CreateTicketOrderViewModel
    {
        public int AttractionId { get; set; }
        public string AttractionName { get; set; }
        public decimal TicketPrice { get; set; }

        [Required(ErrorMessage = "访问日期不能为空")]
        public DateTime VisitDate { get; set; }

        [Required(ErrorMessage = "购票数量不能为空")]
        [Range(1, 100, ErrorMessage = "购票数量必须在1-100之间")]
        public int Quantity { get; set; }
    }

    public class PayTicketOrderViewModel
    {
        public int TicketOrderId { get; set; }
        public string AttractionName { get; set; }
        public DateTime VisitDate { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
    }

    public class TicketOrderDetailViewModel
    {
        public int TicketOrderId { get; set; }
        public string AttractionName { get; set; }
        public DateTime VisitDate { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public string VerifyCode { get; set; }
    }

    public class CreateHotelOrderViewModel
    {
        public int RoomTypeId { get; set; }
        public string HotelName { get; set; }
        public string RoomTypeName { get; set; }
        public decimal PricePerNight { get; set; }

        [Required(ErrorMessage = "入住日期不能为空")]
        public DateTime CheckInDate { get; set; }

        [Required(ErrorMessage = "离店日期不能为空")]
        public DateTime CheckOutDate { get; set; }
    }

    public class PayHotelOrderViewModel
    {
        public int HotelOrderId { get; set; }
        public string HotelName { get; set; }
        public string RoomTypeName { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
    }

    public class HotelOrderDetailViewModel
    {
        public int HotelOrderId { get; set; }
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

    public class UserOrderListViewModel
    {
        public List<TicketOrderListItemViewModel> TicketOrders { get; set; } = new();
        public List<HotelOrderListItemViewModel> HotelOrders { get; set; } = new();
    }

    public class TicketOrderListItemViewModel
    {
        public int TicketOrderId { get; set; }
        public string AttractionName { get; set; }
        public DateTime VisitDate { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class HotelOrderListItemViewModel
    {
        public int HotelOrderId { get; set; }
        public string HotelName { get; set; }
        public string RoomTypeName { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TicketOrdersViewModel
    {
        public List<TicketOrder> Orders { get; set; } = new();
        public string SelectedStatus { get; set; }
    }

    public class HotelOrdersViewModel
    {
        public List<HotelOrder> Orders { get; set; } = new();
        public string SelectedStatus { get; set; }
    }

    public class EditProfileViewModel
    {
        public string Nickname { get; set; }
        public string Bio { get; set; }
        public IFormFile Avatar { get; set; }
    }

    public class EditDiaryViewModel
    {
        public int DiaryId { get; set; }

        [Required(ErrorMessage = "标题不能为空")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "标题长度必须在1-200字符之间")]
        public string Title { get; set; }

        [Required(ErrorMessage = "内容不能为空")]
        [StringLength(5000, MinimumLength = 1, ErrorMessage = "内容长度必须在1-5000字符之间")]
        public string Content { get; set; }
    }
}