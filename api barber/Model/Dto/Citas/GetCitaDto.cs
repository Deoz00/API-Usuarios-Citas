namespace api_barber.Model.Dto.Citas
{
    public class GetCitaDto
    {

        public int Id { get; set; }
        public string Cliente { get; set; }


        public string Empleado { get; set; }


        public DateTime fechaCita { get; set; }
        public string Estado { get; set; }
        public string Comentarios { get; set; }
    }
}
