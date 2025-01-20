namespace api_barber.Model
{
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Mvc;
    using api_barber.Model;
    using System.Net;
    using System.Linq;

    public class ApiResponseFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Este método puede mantenerse vacío si no necesitas verificar nada antes de la ejecución
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Creamos la respuesta predeterminada
            var respuestaApi = new RespuestaApi
            {
                statusCode = HttpStatusCode.OK,  // Suponemos que la respuesta será exitosa por defecto
                isSucess = true,
                ErrorMessages = new List<string>(),
                result = null
            };

            // Si ModelState tiene errores de validación
            if (!context.ModelState.IsValid)
            {
                var errorMessages = context.ModelState
                    .Where(ms => ms.Value.Errors.Count > 0)
                    .SelectMany(ms => ms.Value.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                respuestaApi.statusCode = HttpStatusCode.BadRequest;
                respuestaApi.isSucess = false;
                respuestaApi.ErrorMessages.AddRange(errorMessages); // Agregamos los errores de validación
            }

            // Si se ha producido algún otro error o lógica de negocio
            if (context.Exception != null)
            {
                // Aquí puedes agregar errores adicionales, por ejemplo, errores de la lógica de negocio
                respuestaApi.statusCode = HttpStatusCode.InternalServerError;
                respuestaApi.isSucess = false;
                respuestaApi.ErrorMessages.Add("Ha ocurrido un error en el servidor: " + context.Exception.Message);
                context.ExceptionHandled = true;  // Marca la excepción como manejada
            }

            // Si la acción es exitosa, pero hay algún error adicional
            if (context.Result is ObjectResult objectResult && context.ModelState.IsValid && context.Exception == null)
            {
                var result = objectResult.Value;
                respuestaApi.result = result;
            }

            // Finalmente, establecemos la respuesta modificada
            context.Result = new ObjectResult(respuestaApi)
            {
                StatusCode = (int)respuestaApi.statusCode
            };
        }
    }






}
