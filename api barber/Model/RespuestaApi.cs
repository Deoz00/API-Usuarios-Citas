using System.Net;

namespace api_barber.Model
{
    public class RespuestaApi
    {
        public RespuestaApi()
        {
            ErrorMessages = new List<string>();
        }

        public HttpStatusCode statusCode { get; set; }
        public bool isSucess { get; set; } = true;
        public List<string> ErrorMessages { get; set; }
        public object result { get; set; }

    }
}
