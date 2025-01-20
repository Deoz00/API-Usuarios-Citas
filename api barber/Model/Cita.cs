using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace api_barber.Model
{
    public class Cita
    {
        public int Id { get; set; }

        public string ClienteId { get; set; }
        public User Cliente { get; set; }


        public string EmpleadoId { get; set; }
        public User Empleado { get; set; }


        public DateTime fechaCita { get; set; }
        public string Estado { get; set; }
        public string? Comentarios { get; set; }
    }
}
