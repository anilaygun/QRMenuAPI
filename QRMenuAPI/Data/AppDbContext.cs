using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QRMenuAPI.Models;
using QRMenuAPI.Models.Authentication;

namespace QRMenuAPI.Data
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Company>? Companies { get; set; }
        public DbSet<State>? States { get; set; }
        public DbSet<Restaurant>? Restaurants { get; set; }
        public DbSet<RestaurantUser>? RestaurantUsers { get; set; }
        public DbSet<Category>? Categories { get; set; }
        public DbSet<Food>? Foods { get; set; }
        public DbSet<Menu>? Menus { get; set; }
        public DbSet<FoodMenu>? FoodMenus { get; set; }
        public DbSet<LogEntry> LogEntries { get; set; }
        // public DbSet<AppUserRefreshToken> RefreshTokens => Set<AppUserRefreshToken>();  // token için sonradan eklendi 16.28 bu saaatten önce yapılmış son ekleme değil
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<AppUser>().HasOne(u => u.State).WithMany().OnDelete(DeleteBehavior.NoAction);
            builder.Entity<Company>().HasOne(c => c.State).WithMany().OnDelete(DeleteBehavior.NoAction);
            builder.Entity<Restaurant>().HasOne(r => r.State).WithMany().OnDelete(DeleteBehavior.NoAction);
            builder.Entity<RestaurantUser>().HasOne(ru => ru.Restaurant).WithMany().OnDelete(DeleteBehavior.NoAction);
            builder.Entity<RestaurantUser>().HasOne(ru => ru.AppUser).WithMany().OnDelete(DeleteBehavior.NoAction);
            builder.Entity<Category>().HasOne(c => c.State).WithMany().OnDelete(DeleteBehavior.NoAction);
            builder.Entity<Category>().HasOne(c => c.Restaurant).WithMany().OnDelete(DeleteBehavior.NoAction);
            builder.Entity<Food>().HasOne(f => f.State).WithMany().OnDelete(DeleteBehavior.NoAction);
            builder.Entity<Menu>().HasOne(m => m.State).WithMany().OnDelete(DeleteBehavior.NoAction);
            builder.Entity<Menu>().HasOne(m => m.Food).WithMany().OnDelete(DeleteBehavior.NoAction);
            builder.Entity<Menu>().HasOne(m => m.Category).WithMany().OnDelete(DeleteBehavior.NoAction);
            builder.Entity<FoodMenu>().HasOne(fm => fm.Menu).WithMany().OnDelete(DeleteBehavior.NoAction);
            builder.Entity<FoodMenu>().HasOne(fm => fm.Food).WithMany().OnDelete(DeleteBehavior.NoAction);

            builder.Entity<RestaurantUser>().HasKey(ru => new { ru.RestaurantId, ru.AppUserId });
            builder.Entity<FoodMenu>().HasKey(fm => new { fm.MenuId, fm.FoodId });
            base.OnModelCreating(builder);
        }
    }
}
