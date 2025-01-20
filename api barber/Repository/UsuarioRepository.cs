using api_barber.Data;
using api_barber.Model;
using api_barber.Model.Dto.Usuario;
using api_barber.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace api_barber.Repository
{
    public class UsuarioRepository : IUsuarioRepository
    {

        private readonly ApplicationDbContext _db;
        private string claveSecreta;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public UsuarioRepository(ApplicationDbContext db, IConfiguration config, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            claveSecreta = config.GetValue<string>("ApiSettings:Secreta"); 
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<UsuarioDatosDto> getUsuario(string usuarioId)
        {
            var user = await _db.usuario.FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (user == null)  
            {
                return null; 
            }

            var role = await _userManager.GetRolesAsync(user);

            var usuarioDto = new UsuarioDatosDto
            {
                Name = user.Name,
                UserName = user.UserName,
                role = role.FirstOrDefault()
            };

            return usuarioDto;
        }

        public async Task<ICollection<UsuarioDto>> getUsuarios()
        {
            // Obtenemos los usuarios de la base de datos.
            var users = await _db.usuario.OrderBy(u => u.Id).ToListAsync();

            // Creamos una lista de tareas para obtener los roles en paralelo.
            var usuariosDatosDto = new List<UsuarioDto>();

            foreach (var user in users)
            {
                // Obtener los roles del usuario.
                var roleNames = await _userManager.GetRolesAsync(user);

                // Agregar el DTO del usuario con el primer rol (si existe).
                usuariosDatosDto.Add(new UsuarioDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    UserName = user.UserName,
                    role = roleNames.FirstOrDefault() // Primer rol del usuario (puedes modificar para manejar múltiples roles).
                });
            }

            return usuariosDatosDto;
        }

        public bool isUniqueUser(string usuario)
        {
            var usuariodb = _db.usuario.FirstOrDefault(u => u.UserName == usuario);
            if (usuariodb == null)
            {
                return true;
            }
            return false;
        }

        public  async Task<UsuarioLoginRespuestaDto> registro(UsuarioRegistroDto usuarioRegistroDto, string role)
        {
           


            User usuario = new User()
            {
                UserName = usuarioRegistroDto.UserName,
                Name = usuarioRegistroDto.Name
                // poner los datos necesarios 

            };


            var result = await _userManager.CreateAsync(usuario, usuarioRegistroDto.password);

            if (result.Succeeded)
            {
               
                
                await _userManager.AddToRoleAsync(usuario, role);
                


                var usuarioRetornado = _db.usuario.FirstOrDefault(u => u.UserName == usuarioRegistroDto.UserName);
                var roles = await _userManager.GetRolesAsync(usuarioRetornado);

                var token = GenerateToken(usuarioRetornado, claveSecreta, roles);


                UsuarioLoginRespuestaDto usuarioLoginRespuestaDto = new UsuarioLoginRespuestaDto()
                {
                    Token = token
                };


                return usuarioLoginRespuestaDto;

            }

            UsuarioLoginRespuestaDto respuesta = new UsuarioLoginRespuestaDto();
            respuesta.Token = "";

            return respuesta;
        }

        public async Task<UsuarioLoginRespuestaDto> login(UsuarioLoginDto usuarioLoginDto)
        {
            var usuario = _db.usuario.FirstOrDefault(
            u => u.UserName.ToLower() == usuarioLoginDto.UserName.ToLower());

            bool isValida = await _userManager.CheckPasswordAsync(usuario, usuarioLoginDto.password);

            // validacion si el usuario y contraseña existe

            if (usuario == null || !isValida)
            {
                UsuarioLoginRespuestaDto respuesta = new UsuarioLoginRespuestaDto();
                respuesta.Token = "";
              
                return respuesta;
            }

            // usuario existe - login

            var roles = await _userManager.GetRolesAsync(usuario);


            var token = GenerateToken(usuario, claveSecreta, roles);

            UsuarioLoginRespuestaDto usuarioLoginRespuestaDto = new UsuarioLoginRespuestaDto()
            {
                Token = token
            };


            return usuarioLoginRespuestaDto;




        }

        public static string GenerateToken(User usuario, string claveSecreta, IList<string> roles)
        {
            // Inicializar el manejador de tokens
            var manejadorToken = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(claveSecreta);

            // Configuración del descriptor del token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim(ClaimTypes.Name, usuario.UserName ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Role, roles.FirstOrDefault()) // Toma el primer rol
            }),
                Expires = DateTime.UtcNow.AddDays(7), // Configuración del tiempo de expiración
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            // Crear el token
            var token = manejadorToken.CreateToken(tokenDescriptor);

            // Retornar el token como cadena
            return manejadorToken.WriteToken(token);
        }


        public string GetLoggedUserId()
        {

            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;


          
            return userId;
        }

        public async Task<ICollection<UsuarioIdDto>> getUsuariosByRol(string rol)
        {

            var users = await _userManager.GetUsersInRoleAsync(rol); // Obtiene usuarios en el rol

            var usuariosDto = users.Select(u => new UsuarioIdDto
            {
                Name = u.Name,
                id = u.Id,
                citasEmpleado = _db.Citas
                 .Where(c => c.EmpleadoId == u.Id)
                 .Select(c => c.fechaCita)  // Solo selecciona el campo de la fecha
                 .ToList()
            }).ToList();

            return usuariosDto;


        }

        public async Task<bool> existRole(string role)
        {
            var roleExists = await _roleManager.RoleExistsAsync(role);
            if (!roleExists)
            {
              return false;
            }
            return true;
        }

        public async Task<bool> delete(string userId)
        {
            // Obtén el usuario por su ID
            var usuario = await _userManager.FindByIdAsync(userId);
            if (usuario == null)
            {
               
                return false;
            }

            var resultado = await _userManager.DeleteAsync(usuario);
            return resultado.Succeeded;
        }


        public async Task<bool> Edit(string userId, UsuarioEditDto usuarioEditDto)
        {
            // Obtén el usuario por su ID
            var usuario = await _userManager.FindByIdAsync(userId);
            if (usuario == null)
            {
                // El usuario no existe
                return false;
            }

            // Actualiza UserName y Name
            usuario.UserName = usuarioEditDto.UserName;
            usuario.NormalizedUserName = usuarioEditDto.UserName.ToUpper(); // Normalización
            usuario.Name = usuarioEditDto.Name;

            var resultadoUsuario = await _userManager.UpdateAsync(usuario);
            if (!resultadoUsuario.Succeeded)
            {
                return false;
            }

            // Cambia el Rol del usuario
            var rolesActuales = await _userManager.GetRolesAsync(usuario);
            if (rolesActuales.Count > 0)
            {
                // Elimina los roles actuales
                await _userManager.RemoveFromRolesAsync(usuario, rolesActuales);
            }

            // Verifica si el nuevo rol existe
            string rolAsignar = usuarioEditDto.role;
            if (!await _roleManager.RoleExistsAsync(usuarioEditDto.role))
            {
                // Si el rol no existe, usa "Cliente"
                rolAsignar = "Cliente";

                // Verifica si el rol "Cliente" existe, si no, créalo
                if (!await _roleManager.RoleExistsAsync(rolAsignar))
                {
                    var nuevoRol = new IdentityRole(rolAsignar);
                    var resultadoRolCreacion = await _roleManager.CreateAsync(nuevoRol);
                    if (!resultadoRolCreacion.Succeeded)
                    {
                        return false; // Falló la creación del rol
                    }
                }
            }

            // Agrega el nuevo rol al usuario
            var resultadoRol = await _userManager.AddToRoleAsync(usuario, rolAsignar);
            if (!resultadoRol.Succeeded)
            {
                return false;
            }

            return true;
        }
    }
}
