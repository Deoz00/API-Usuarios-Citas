using api_barber.Model.Dto.Usuario;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace api_barber.Repository.IRepository
{
    public interface IUsuarioRepository
    {
        Task<ICollection<UsuarioDto>> getUsuarios();

        Task<ICollection<UsuarioIdDto>> getUsuariosByRol(string rol);


        Task<UsuarioDatosDto> getUsuario(string usuarioId);


        bool isUniqueUser(string usuario);


        Task<UsuarioLoginRespuestaDto> login(UsuarioLoginDto usuarioLoginDto);


        Task<UsuarioLoginRespuestaDto> registro(UsuarioRegistroDto usuarioRegistroDto, string role);
        string GetLoggedUserId();

        Task<bool> existRole(string role);

        Task<bool> delete(string userId);
        Task<bool> Edit(string userId, UsuarioEditDto usuarioEditDto);
    }
        
}
