using api_barber.Data;
using api_barber.Model;
using api_barber.Model.Dto.Citas;
using api_barber.Repository.IRepository;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace api_barber.Repository
{
    public class CitasRepository : ICitasRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;


        public CitasRepository(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }



        public async Task<bool> crearCita(Cita cita)
        {
            //var userId = this.user//User.FindFirstValue(ClaimTypes.NameIdentifier); // Obtener el ID del usuario desde el JWT



            _db.Citas.Add(cita);
            return await guardar();
        }

        public async Task<GetCitaDto> getCita(int citaId)
        {
            var cita = await _db.Citas
                .Where(c => c.Id == citaId)
                .Select(c => new GetCitaDto
                {
                    Cliente = c.Cliente.UserName,
                    Empleado = c.Empleado.UserName,
                    fechaCita = c.fechaCita,
                    Estado = c.Estado,
                    Comentarios = c.Comentarios
                })
                .FirstOrDefaultAsync();


            if (cita == null)
            {
                return null;
            }




            return cita;
        }


        public async Task<ICollection<GetCitaDto>> getCitas()
        {
            var cita = await _db.Citas
                .Select(c => new GetCitaDto
                {
                    Cliente = c.Cliente.UserName,
                    Empleado = c.Empleado.UserName,
                    fechaCita = c.fechaCita,
                    Estado = c.Estado,
                    Comentarios = c.Comentarios
                }).ToListAsync();

            return cita;


            //return await _db.Citas.OrderBy(c => c.fechaCita).ToListAsync();
        }



        public async Task<bool> actualizarCita(Cita cita)
        {



            _db.Citas.Update(cita);
            return await guardar();
        }


        public async Task<bool> guardar()
        {
            return await _db.SaveChangesAsync() >= 0 ? true : false;
        }

        public async Task<Cita> getCitaModel(int citaId)
        {
            var cita = await _db.Citas.FirstOrDefaultAsync(u => u.Id == citaId);



            if (cita == null)
            {
                return null;
            }




            return cita;
        }

        public async Task<ICollection<GetCitaDto>> getCitasUsuario(string userId, bool isCliente)
        {
            var citasQuery = _db.Citas.AsQueryable();


            if (isCliente)
            {
                citasQuery = citasQuery.Where(c => c.ClienteId == userId);

            }
            else
            {
                citasQuery = citasQuery.Where(c => c.EmpleadoId == userId);
            }

            var cita = await citasQuery
                .Select(c => new GetCitaDto
                {
                    Id = c.Id,
                    Cliente = c.Cliente.UserName,
                    Empleado = c.Empleado.UserName,
                    fechaCita = c.fechaCita,
                    Estado = c.Estado,
                    Comentarios = c.Comentarios
                }).ToListAsync();

            return cita;
        }

        public async Task<bool> deleteCita(int citaId)
        {
            var cita = await _db.Citas.FindAsync(citaId);

            if (cita == null)
            {
                return false;
            }

            // Eliminar la cita
            _db.Citas.Remove(cita);

            return await guardar();

        }
    }
}
