using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ProjectAPI.Shared.GeneralDTO;
using System.Linq;

namespace API.Attributes
{
    public class ValidarModeloAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errores = context.ModelState
                    .SelectMany(x => x.Value!.Errors)
                    .Select(x => x.ErrorMessage)
                    .ToArray();

                var respuesta = RespuestaDto.ParametrosIncorrectos(
                    "Datos inválidos",
                    string.Join("; ", errores));

                context.Result = new BadRequestObjectResult(respuesta);
            }
        }
    }
}
