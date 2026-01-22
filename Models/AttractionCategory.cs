// namespace TourismPlatform.Models
// {
//     public class AttractionCategory
//     {
//         public int CategoryId { get; set; }
//         public string Name { get; set; }
//         public string Description { get; set; }

//         // Navigation properties
//         public ICollection<Attraction> Attractions { get; set; } = new List<Attraction>();
//     }
// }


using System.ComponentModel.DataAnnotations; // <--- 1. 必须添加这个引用

namespace TourismPlatform.Models
{
    public class AttractionCategory
    {
        [Key] // <--- 2. 在这里添加 Key 特性
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        // Navigation properties
        public ICollection<Attraction> Attractions { get; set; } = new List<Attraction>();
    }
}