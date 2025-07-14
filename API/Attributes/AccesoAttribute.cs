using Business.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ProjectAPI.Shared.GeneralDTO;
using System.Linq;
using System.Threading.Tasks;

namespace API.Attributes
{
    /// <summary>
    /// Atributo que valida el acceso a la API mediante headers Sitio y Clave
    /// </summary>
    public class AccesoAttribute : ActionFilterAttribute
    {
        private readonly IAuthRepository _authRepository;

        public AccesoAttribute(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            string sitio = context.HttpContext.Request.Headers["Sitio"].FirstOrDefault() ?? string.Empty;
            string clave = context.HttpContext.Request.Headers["Clave"].FirstOrDefault() ?? string.Empty;

            System.Diagnostics.Debug.WriteLine($"AccesoAttribute - Sitio: {sitio}, Clave: {clave}");

            // Validar que los headers estén presentes
            if (string.IsNullOrEmpty(sitio) || string.IsNullOrEmpty(clave))
            {
                context.Result = new ObjectResult(RespuestaDto.ParametrosIncorrectos(
                    "Headers requeridos",
                    "Los headers 'Sitio' y 'Clave' son requeridos para acceder a la API"))
                {
                    StatusCode = 401
                };
                return;
            }

            // Validar credenciales de acceso
            if (!await _authRepository.ValidarAccesoAsync(sitio, clave))
            {
                context.Result = new ObjectResult(RespuestaDto.ParametrosIncorrectos(
                    "Acceso inválido",
                    "Las credenciales de acceso son inválidas"))
                {
                    StatusCode = 401
                };
                return;
            }

            // Si la validación es exitosa, continuar con la ejecución
            await next();
        }
    }
}
