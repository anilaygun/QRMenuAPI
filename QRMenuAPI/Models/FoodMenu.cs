using System.ComponentModel.DataAnnotations.Schema;

namespace QRMenuAPI.Models
{
    public class FoodMenu
    {
        [ForeignKey("FoodId")]
        public int? FoodId { get; set; }
        public Food? Food { get; set; }

        [ForeignKey("MenuId")]
        public int? MenuId { get; set; }
        public Menu? Menu { get; set; }
    }
}
