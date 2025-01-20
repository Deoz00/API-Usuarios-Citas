using api_barber.Model;
using api_barber.Model.Dto.Usuario;
using api_barber.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;
using System.Security.Claims;

namespace api_barber.Controllers
{
    [Route("api/usuario")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioRepository _usRepo;
        protected RespuestaApi _respuestaApi;
        private readonly IMapper _mapper;

        public UsuarioController(IUsuarioRepository usRepo, IMapper mapper)
        {
            _usRepo = usRepo;
            _respuestaApi = new();
            _mapper = mapper;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUsers()
        {
            var userList = await _usRepo.getUsuarios();


            if (userList == null || !userList.Any())
            {   
                ModelState.AddModelError("Error", "No se encontraron usuarios.");
                return NotFound();
            }
            return Ok(userList);
        }


        [Authorize(Roles = "Admin,Cliente,Empleado")]
        [HttpGet("myuser")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMyUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _usRepo.getUsuario(userId);

            if (user == null)
            {
                ModelState.AddModelError("Error", "No se encontraron usuarios.");
                return NotFound();
            }
            return Ok(user);

        }



        [Authorize(Roles = "Admin,Cliente,Empleado")]
        [HttpGet("rol/{rol}", Name = "GetUsersByRole")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUsersByRole(string rol)
        {

            var userList = await _usRepo.getUsuariosByRol(rol);

            if (userList == null || !userList.Any())
            {
                ModelState.AddModelError("Error", "No se encontraron usuarios.");
                return NotFound();
            }
            return Ok(userList);

        }





        [Authorize(Roles = "Admin")]
        [HttpGet("{id}", Name = "GetUser")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _usRepo.getUsuario(id);

            if (user == null)
            {
                ModelState.AddModelError("Error", "No se encontraron usuarios.");
                return NotFound();
            }
            return Ok(user);

        }


        [AllowAnonymous]
        [HttpPost("registro")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> registro([FromBody] UsuarioRegistroDto usuarioRegistroDto)
        {
       
            bool validarUsuarioUnico = _usRepo.isUniqueUser(usuarioRegistroDto.UserName);

            if (!validarUsuarioUnico)
            {
                ModelState.AddModelError("Error", "El nombre de usuario ya existe");
                return BadRequest();
            }

            var usuario = await _usRepo.registro(usuarioRegistroDto, "Cliente");

            if (usuario==null)
            {
                ModelState.AddModelError("Error", "Error al registar");

                return BadRequest(_respuestaApi);
            }

            return Ok(usuario);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("admin/registro")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> registro([FromBody] UsuarioRegistroDto usuarioRegistroDto, string role)
        {
            bool validarUsuarioUnico = _usRepo.isUniqueUser(usuarioRegistroDto.UserName);

            
            if (!(await _usRepo.existRole(role)))
            {
                ModelState.AddModelError("Error", "El rol no existe");
                return BadRequest();


            }

            if (!validarUsuarioUnico)
            {
                ModelState.AddModelError("Error", "El nombre de usuario ya existe");

                return BadRequest();
            }

            var usuario = await _usRepo.registro(usuarioRegistroDto, role);

            if (usuario == null)
            {
                ModelState.AddModelError("Error", "Error al registar");
                return BadRequest(_respuestaApi);
            }

            return Ok();
        }



        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> login([FromBody] UsuarioLoginDto usuarioLoginDto)
        {
            var respuestaLogin = await _usRepo.login(usuarioLoginDto);

            if (respuestaLogin == null || string.IsNullOrEmpty(respuestaLogin.Token))
            {
               
                ModelState.AddModelError("Error", "El nombre de usuario o contraseña es incorrecta");
                return BadRequest();
            }

            
            return Ok(respuestaLogin);
        }






        [Authorize(Roles = "Admin")]
        [HttpPatch("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EditarUsuario(string userId, [FromBody] UsuarioEditDto usuarioEditDto)
        {
            bool validarUsuarioUnico = _usRepo.isUniqueUser(usuarioEditDto.UserName);


            if (!validarUsuarioUnico)
            {
                ModelState.AddModelError("Error", "El nombre de usuario ya existe");

                return BadRequest();
            }


            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var resultado = await _usRepo.Edit(userId, usuarioEditDto);
            if (resultado)
            {
                return Ok("Usuario editado correctamente.");
            }
            return BadRequest("No se pudo editar el usuario.");
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EliminarUsuario(string userId)
        {
            var resultado = await _usRepo.delete(userId);
            if (resultado)
            {
                return Ok("Usuario eliminado correctamente.");
            }
            return BadRequest("No se pudo eliminar el usuario.");
        }


    }
}
