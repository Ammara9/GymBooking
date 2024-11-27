using System.Security.Claims;
using GymBooking.Models;
using Microsoft.AspNetCore.Identity;

namespace GymBooking.Data
{
    public class SeedData
    {
        private static ApplicationDbContext context = default!;
        private static RoleManager<IdentityRole> roleManager = default!;
        private static UserManager<ApplicationUser> userManager = default!;

        public static async Task Init(ApplicationDbContext _context, IServiceProvider services)
        {
            context = _context;
            if (context.Roles.Any())
                return;

            roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            var roleNames = new[] { "User", "Admin" };
            var adminEmail = "admin@Gymbokning.se";
            var userEmail = "user@Gymbokning.se";

            await AddRolesAsync(roleNames);

            var admin = await AddAccountAsync(
                adminEmail,
                "Abcd_1234",
                "AdminFirstName",
                "AdminLastName"
            );
            var user = await AddAccountAsync(
                userEmail,
                "Abcd_1234",
                "UserFirstName",
                "UserLastName"
            );

            await AddUserToRoleAsync(admin, "Admin");
            await AddUserToRoleAsync(user, "User");
        }

        private static async Task AddUserToRoleAsync(ApplicationUser user, string roleName)
        {
            if (!await userManager.IsInRoleAsync(user, roleName))
            {
                var result = await userManager.AddToRoleAsync(user, roleName);
                if (!result.Succeeded)
                    throw new Exception(string.Join("\n", result.Errors));
            }
        }

        private static async Task AddRolesAsync(string[] roleNames)
        {
            foreach (var roleName in roleNames)
            {
                if (await roleManager.RoleExistsAsync(roleName))
                    continue;
                var role = new IdentityRole { Name = roleName };
                var result = await roleManager.CreateAsync(role);

                if (!result.Succeeded)
                    throw new Exception(string.Join("\n", result.Errors));
            }
        }

        private static async Task<ApplicationUser> AddAccountAsync(
            string accountEmail,
            string pw,
            string firstName,
            string lastName
        )
        {
            var found = await userManager.FindByEmailAsync(accountEmail);

            if (found != null)
                return null!;

            var user = new ApplicationUser
            {
                UserName = accountEmail,
                Email = accountEmail,
                FirstName = firstName,
                LastName = lastName,
                TimeOfRegistration = DateTime.Now,
                EmailConfirmed = true,
            };

            var result = await userManager.CreateAsync(user, pw);

            if (!result.Succeeded)
                throw new Exception(string.Join("\n", result.Errors));

            //name on profile....
            var fullName = $"{firstName} {lastName}";
            await userManager.AddClaimAsync(user, new Claim("FullName", fullName));

            return user;
        }
    }
}
