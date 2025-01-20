using System.ComponentModel.DataAnnotations;

namespace api_barber.Model.Dto.Usuario
{
    public class UsuarioLoginDto
    {
        [Required  (ErrorMessage = "El usuario es obligatorio")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string password { get; set; }
    }
}
