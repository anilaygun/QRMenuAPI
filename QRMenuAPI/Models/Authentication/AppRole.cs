
using Microsoft.AspNetCore.Identity;

namespace QRMenuAPI.Models.Authentication
{
    public class AppRole:IdentityRole
    {
        public DateTime RegisterDate { get; set; }
    }
}
