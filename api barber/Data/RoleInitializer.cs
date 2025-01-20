using api_barber.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace api_barber.Data
{
    public class RoleInitializer
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;


        public RoleInitializer(RoleManager<IdentityRole> roleManager, UserManager<User> userManager, IConfiguration configuration)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task InitializeRolesAsync()
        {
            var roles = new[] { "Admin", "Empleado", "Cliente" };

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Crear usuario administrador si no existe
            var user = _configuration["AdminUser:User"];
            var name = _configuration["AdminUser:Name"];
            var password = _configuration["AdminUser:Password"];

            var adminUser = await _userManager.FindByNameAsync(user);
            if (adminUser == null)
            {
                adminUser = new User
                {
                    UserName = user,
                    Name = name
                    
                };

                var result = await _userManager.CreateAsync(adminUser, password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }




        }

    }

}
