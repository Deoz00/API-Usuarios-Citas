using api_barber.Mappers;
using api_barber.Model;
using api_barber.Model.Dto.Citas;
using api_barber.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net;
using System.Security.Claims;

namespace api_barber.Controllers
{
    [Route("api/Citas")]
    [ApiController]
    public class CitasController : ControllerBase
    {

        private readonly ICitasRepository _citasRepo;
        private readonly IUsuarioRepository _usRepo;
        private readonly IMapper _mapper;
        protected RespuestaApi _respuestaApi;


        public CitasController(ICitasRepository citasRepo, IMapper mapper, IUsuarioRepository usRepo)
        {
            _citasRepo = citasRepo;
            _mapper = mapper;
            _usRepo = usRepo;
            _respuestaApi = new();

        }

        [Authorize(Roles = "Admin,Cliente,Empleado")]
        [HttpGet("{citaId:int}", Name = "GetCita")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public async Task<IActionResult> GetCita(int citaId)
        {
            var itemCita = await _citasRepo.getCita(citaId);

            if (itemCita == null)
            {
                
                ModelState.AddModelError("Error", "No se encontraron citas.");
                return NotFound();
            }

            return Ok(itemCita);

        }


        [Authorize(Roles = "Admin,Cliente,Empleado")]
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(CrearCitaDto))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> crearCita([FromBody] CrearCitaDto citasDto)
        {
            var userId = _usRepo.GetLoggedUserId();
            var citaDto2 = new CitasDto
            {
                fechaCita = citasDto.fechaCita,
                EmpleadoId = citasDto.EmpleadoId,
                ClienteId = userId,
                Estado = "En espera"


            };


       
            var cita = _mapper.Map<Cita>(citaDto2);

            if ( !(await _citasRepo.crearCita(cita)))
            {
                ModelState.AddModelError("", "Algo salió mal guardando el registro");
                return StatusCode(500);
            }

            return Ok(citasDto);
        }




        [Authorize(Roles = "Admin,Cliente,Empleado")]
        [HttpPatch("{citaId}", Name = "editCita")]
        [ProducesResponseType(204)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EditCita(int citaId, [FromBody] EditCitaDto citasDto)
        {
          

            var getcita = await _citasRepo.getCitaModel(citaId);

            if (getcita == null)
            {
                ModelState.AddModelError("Error", "La cita no fue encontrada.");
                return NotFound();
            }

            getcita.EmpleadoId = citasDto.EmpleadoId;

            // Actualizar propiedades si están presentes en el DTO
            if (citasDto.fechaCita.HasValue)
            {
                getcita.fechaCita = citasDto.fechaCita.Value;
            }

            if (!string.IsNullOrEmpty(citasDto.Estado))
            {
                getcita.Estado = citasDto.Estado;
            }

            if (citasDto.Comentarios != null) // Permitir null para eliminar comentarios
            {
                getcita.Comentarios = citasDto.Comentarios;
            }

            // Guardar cambios
            if (!await _citasRepo.actualizarCita(getcita))
            {
                ModelState.AddModelError("", $"Algo salió mal actualizando la cita");
                return StatusCode(500);
            }

            return NoContent();
        }



        [Authorize(Roles = "Admin")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCitas()
        {
            var citas = await _citasRepo.getCitas();

            var citasDto = new List<GetCitaDto>();

            foreach (var lista in citas)
            {
                citasDto.Add(_mapper.Map<GetCitaDto>(lista));
            }

            return Ok(citasDto);
        }


        [Authorize(Roles = "Admin,Cliente,Empleado")]
        [HttpGet("my")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyCitas()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var userRole = User.FindFirstValue(ClaimTypes.Role);

            bool isCliente = userRole == "Cliente";
            var citas = await _citasRepo.getCitasUsuario(userId, isCliente);

            var citasDto = new List<GetCitaDto>();

            foreach (var lista in citas)
            {
                citasDto.Add(_mapper.Map<GetCitaDto>(lista));
            }

            return Ok(citasDto);


        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCita(int id)
        {
            var data = await _citasRepo.deleteCita(id);

            if  (data == false)
            {
                return BadRequest();
            }

            return Ok();
          
        }
    }
}
