using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using QRMenuAPI.CustomValidations;
using QRMenuAPI.Data;
using QRMenuAPI.Models.Authentication;
using QRMenuAPI.Services;

//using QRMenuAPI.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    options.Password.RequiredLength = 5;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireDigit = false;
    //
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abc�defghi�jklmno�pqrs�tu�vwxyzABC�DEFGHI�JKLMNO�PQRS�TU�VWXYZ0123456789-._@+";
})
    .AddPasswordValidator<CustomPasswordValidation>()
    .AddUserValidator<CustomUserValidation>()
    .AddErrorDescriber<CustomIdentityErrorDescriber>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

//builder.Services
//    .AddIdentityApiEndpoints<AppUser>()
//    .AddRoleManager<AppRole>()
//    .AddEntityFrameworkStores<AppDbContext>()
//    .AddDefaultTokenProviders();
//;


//builder.Services.ConfigureApplicationCookie(options =>
//{
//    options.LoginPath = new PathString("/AppUsers/Login");
//    options.LogoutPath = new PathString("/AppUsers/Logout"); // Kullan�c� ��k�� yapt���nda y�nlendirilecek yol
//    options.Cookie = new CookieBuilder
//    {
//        Name = "AspNetCoreIdentityCookie",
//        HttpOnly = false, // XSS sald�r�lar�na kar�� koruma sa�lar.
//        SameSite = SameSiteMode.Lax, // CSRF sald�r�lar�na kar�� koruma sa�lar.
//        SecurePolicy = CookieSecurePolicy.Always // HTTPS �zerinden cookie g�nderilmesini sa�lar.
//    };
//    options.SlidingExpiration = true; // Cookie'nin s�resi, kullan�c� etkin oldu�u s�rece yenilenir.
//    options.ExpireTimeSpan = TimeSpan.FromMinutes(2); // Cookie'nin s�resini 2 dakika olarak ayarlar.
//   
//});

//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//}).AddJwtBearer(options =>
//{
//    options.TokenValidationParameters = new TokenValidationParameters()
//    {
//        ValidateActor = false,
//        ValidateIssuer = false,
//        ValidateAudience = false,
//        RequireExpirationTime = true,
//        ValidateIssuerSigningKey = true,
//        ClockSkew = TimeSpan.Zero,
//        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("Jwt:Key").Value)),

//    };
//});


//builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddScoped<ILogService, LogService>();

builder.Services.AddAuthentication();  // JWT yerine
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.MapIdentityApi<AppUser>(); !!!!!! AddIdentityApiEndpoints !!!!!!
app.UseStatusCodePages(); // ?
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
//Initial data;
AppDbContext? context = app.Services.CreateScope().ServiceProvider.GetService<AppDbContext>();
UserManager<AppUser>? userManager = app.Services.CreateScope().ServiceProvider.GetService<UserManager<AppUser>>();
RoleManager<AppRole>? roleManager = app.Services.CreateScope().ServiceProvider.GetService<RoleManager<AppRole>>();
DBInitializer dBInitializer = new DBInitializer(context, roleManager, userManager);

app.Run();
