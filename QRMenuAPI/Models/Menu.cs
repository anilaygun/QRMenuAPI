using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QRMenuAPI.Models
{
    public class Menu
    {
        [Key]
        public int Id { get; set; }
        [StringLength(30, MinimumLength = 1)]
        [Column(TypeName = "nvarchar(30)")]
        public string Title { get; set; } = "";
        [Range(0, float.MaxValue)]
        public float Price { get; set; }
        [StringLength(250)]
        [Column(TypeName = "nvarchar(250)")]
        public string? Description { get; set; }

        [ForeignKey("FoodId")]
        public int FoodId { get; set; }
        public Food? Food { get; set; }

        [ForeignKey("CategoryId")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public byte StateId { get; set; }
        [ForeignKey("StateId")]
        public State? State { get; set; }
        
        //Base64 - Parent prop
    }
}
