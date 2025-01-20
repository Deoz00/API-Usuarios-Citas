using api_barber.Model;
using api_barber.Model.Dto.Citas;
using api_barber.Model.Dto.Usuario;

namespace api_barber.Repository.IRepository
{
    public interface ICitasRepository
    {

        Task<ICollection<GetCitaDto>> getCitas();

        Task<ICollection<GetCitaDto>> getCitasUsuario(string userId, bool isCliente);



        Task<GetCitaDto> getCita(int citaId);
        Task<Cita> getCitaModel(int citaId);


        // bool isUniqueUser(string usuario);


        Task<bool> crearCita(Cita cita);
        Task<bool> actualizarCita(Cita cita);

        Task<bool> deleteCita(int citaId);


    }
}
