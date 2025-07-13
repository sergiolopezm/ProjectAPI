using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace API.Attributes
{
    public class LogAttribute : ActionFilterAttribute
    {
        private readonly ILogger<LogAttribute> _logger;

        public LogAttribute(ILogger<LogAttribute> logger)
        {
            _logger = logger;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var usuario = context.HttpContext.User.Identity?.Name ?? "Anónimo";
            var action = context.ActionDescriptor.DisplayName;
            var ip = context.HttpContext.Connection.RemoteIpAddress?.ToString();

            _logger.LogInformation("Usuario: {Usuario}, IP: {IP}, Action: {Action}", usuario, ip, action);
        }
    }
}
