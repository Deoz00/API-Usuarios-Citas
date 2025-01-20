using System.ComponentModel.DataAnnotations;

namespace api_barber.Model.Dto.Usuario
{
    public class UsuarioRegistroDto
    {
        [Required(ErrorMessage = "El usuario es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre de usuario no puede superar los 50 caracteres.")]

        public string UserName { get; set; }




        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, ErrorMessage = "La contraseña debe tener mas de {2} caracteres.", MinimumLength = 8)]

        public string password { get; set; }




        [Required(ErrorMessage = "La Nombre es obligatoria")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string Name { get; set; }
    }
}
