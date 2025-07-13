using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using ProjectAPI.Shared.GeneralDTO;

namespace API.Attributes
{
    public class ExceptionAttribute : ExceptionFilterAttribute
    {
        private readonly ILogger<ExceptionAttribute> _logger;

        public ExceptionAttribute(ILogger<ExceptionAttribute> logger)
        {
            _logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "Error no controlado en {Action}",
                context.ActionDescriptor.DisplayName);

            var respuesta = RespuestaDto.ErrorInterno("Error interno del servidor");
            context.Result = new ObjectResult(respuesta) { StatusCode = 500 };
            context.ExceptionHandled = true;
        }
    }
}
