using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectAPI.Util;
using System.Linq;

namespace API.Attributes
{
    public class JwtAuthorizationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var token = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (string.IsNullOrEmpty(token))
            {
                context.Result = new UnauthorizedObjectResult("Token requerido");
                return;
            }

            try
            {
                var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
                var key = configuration["JwtSettings:Key"];
                var issuer = configuration["JwtSettings:Issuer"];
                var audience = configuration["JwtSettings:Audience"];

                var principal = JwtHelper.ValidateToken(token, key!, issuer!, audience!);
                context.HttpContext.User = principal;
            }
            catch
            {
                context.Result = new UnauthorizedObjectResult("Token inválido");
            }
        }
    }
}
