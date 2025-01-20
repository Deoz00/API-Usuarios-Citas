using Microsoft.AspNetCore.Identity;

namespace api_barber.Model
{
    public class User: IdentityUser
    {
        public string Name { get; set; }

        public ICollection<Cita> citasCliente { get; set; }
        public ICollection<Cita> citasEmpleado { get; set; }


    }
}
