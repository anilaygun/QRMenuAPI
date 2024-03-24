using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QRMenuAPI.Models
{
    public class Restaurant
    {
        public int Id { get; set; }
        [StringLength(100, MinimumLength = 2)]
        [Column(TypeName = "nvarchar(100)")]
        public string Name { get; set; } = "";
        [StringLength(5, MinimumLength = 5)]
        [Column(TypeName = "char(5)")]
        [DataType(DataType.PostalCode)]
        public string PostalCode { get; set; } = "";
        [StringLength(200, MinimumLength = 5)]
        [Column(TypeName = "nvarchar(200)")]
        public string Address { get; set; } = "";
        [Phone]
        [Column(TypeName = "varchar(30)")]
        public string Phone { get; set; } = "";
        [EmailAddress]
        [Column(TypeName = "varchar(100)")]
        public string Email { get; set; } = "";
        [Column(TypeName = "smalldatetime")]
        public DateTime RegisterDate { get; set; }
        [StringLength(100)]
        [Column(TypeName = "varchar(11)")]
        public string WebAddress { get; set; } = "";
        public byte StateId { get; set; }
        [ForeignKey("StateId")]
        public State? State { get; set; }
        public int CompanyId { get; set; }
        [ForeignKey("CompanyId")]
        public Company? Company { get; set; }
    }
}
