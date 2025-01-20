using api_barber.Model;
using api_barber.Model.Dto.Citas;
using api_barber.Model.Dto.Usuario;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace api_barber.Mappers
{
    public class Mapper: Profile
    {
        public Mapper()
        {
            CreateMap<IdentityUser, UsuarioDatosDto>().ReverseMap();
            CreateMap<IdentityUser, UsuarioDto>().ReverseMap();

            CreateMap<Cita, CitasDto>().ReverseMap();

            CreateMap<Cita, CrearCitaDto>().ReverseMap();
            CreateMap<Cita, GetCitaDto>().ReverseMap();


        }
    }
}
