namespace api_barber.Model.Dto.Citas
{
    public class EditCitaDto
    {
        public string EmpleadoId { get; set; }
        public DateTime? fechaCita { get; set; }
        public string? Estado { get; set; }
        public string? Comentarios { get; set; }
    }
}
