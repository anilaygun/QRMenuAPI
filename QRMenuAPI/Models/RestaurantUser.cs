using QRMenuAPI.Models.Authentication;
using System.ComponentModel.DataAnnotations.Schema;

namespace QRMenuAPI.Models
{
    public class RestaurantUser
    {
        public int? RestaurantId { get; set; }
        public string? AppUserId { get; set; }
        [ForeignKey("RestaurantId")]
        public Restaurant? Restaurant { get; set; }
        [ForeignKey("AppUserId")]
        public AppUser? AppUser { get; set; }
    }
}
