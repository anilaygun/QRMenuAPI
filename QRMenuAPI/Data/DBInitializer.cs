using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QRMenuAPI.Models;
using QRMenuAPI.Models.Authentication;

namespace QRMenuAPI.Data
{
    public class DBInitializer
    {
        public DBInitializer(AppDbContext? context, RoleManager<AppRole>? roleManager, UserManager<AppUser>? userManager)
        {
            State state;
            AppRole appRole;
            AppUser appUser;
            Company? company = null;


            if (context != null)
            {
                context.Database.Migrate();
                if (context.States.Count() == 0)
                {
                    state = new State();
                    state.Id = 0;
                    state.Name = "Deleted";
                    context.States.Add(state);
                    state = new State();
                    state.Id = 1;
                    state.Name = "Active";
                    context.States.Add(state);
                    state = new State();
                    state.Id = 2;
                    state.Name = "Passive";
                    context.States.Add(state);
                }
                if (context.Companies.Count() == 0)
                {
                    company = new Company();
                    company.Address = "adres";
                    company.Email = "company@admin.com";
                    company.Name = "Company1";
                    company.Phone = "0000000001";
                    company.PostalCode = "00001";
                    company.RegisterDate = DateTime.Today;
                    company.StateId = 1;
                    company.TaxNumber = "11111111111";
                    context.Companies.Add(company);
                }

                if (roleManager != null)
                {
                    if (roleManager.Roles.Count() == 0)
                    {
                        appRole = new AppRole();
                        appRole.Name = "Administrator";
                        roleManager.CreateAsync(appRole).Wait();
                        appRole = new AppRole();
                        appRole.Name = "CompanyAdministrator";
                        roleManager.CreateAsync(appRole).Wait();
                    }
                }

                if (userManager != null)
                {
                    if (userManager.Users.Count() == 0)
                    {
                        if (company != null)
                        {
                            appUser = new AppUser();
                            appUser.UserName = "Administrator";
                            appUser.CompanyId = company.Id;
                            appUser.Name = "Administrator";
                            appUser.Email = "user@admin.com";
                            appUser.PhoneNumber = "111222333";
                            appUser.RegisterDate = DateTime.Today;
                            appUser.StateId = 1;
                            userManager.CreateAsync(appUser, "Admin123!").Wait();
                            userManager.AddToRoleAsync(appUser, "Administrator").Wait();
                        }
                    }
                }
            }
            context.SaveChanges();
        }
    }
}
