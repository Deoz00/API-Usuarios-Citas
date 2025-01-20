namespace api_barber.Model.Dto.Usuario
{
    public class UsuarioIdDto
    {
        
            public string Name { get; set; }
            public string id { get; set; }

        public ICollection<DateTime> citasEmpleado { get; set; }



    }

}

