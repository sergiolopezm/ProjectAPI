using API.Attributes;
using Business.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjectAPI.Shared.AuthDTO;
using ProjectAPI.Shared.GeneralDTO;
using System;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(LogAttribute))]
    [ServiceFilter(typeof(ExceptionAttribute))]
    [ProducesResponseType(typeof(RespuestaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespuestaDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(RespuestaDto), StatusCodes.Status500InternalServerError)]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;

        public AuthController(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        /// <summary>
        /// Autentica un usuario en el sistema
        /// </summary>
        /// <param name="loginDto">Datos de login del usuario</param>
        /// <returns>Respuesta con token JWT si la autenticación es exitosa</returns>
        [HttpPost("login")]
        [ServiceFilter(typeof(AccesoAttribute))]
        [ServiceFilter(typeof(ValidarModeloAttribute))]
        public async Task<ActionResult<RespuestaDto>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                // Obtener IP del cliente
                loginDto.Ip = HttpContext.Connection.RemoteIpAddress?.ToString();

                // Autenticar usuario
                var resultado = await _authRepository.AutenticarAsync(loginDto);

                if (resultado.Exito)
                {
                    return Ok(resultado);
                }
                else
                {
                    return Unauthorized(resultado);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, RespuestaDto.ErrorInterno(
                    $"Error interno del servidor: {ex.Message}"));
            }
        }

        /// <summary>
        /// Registra un nuevo usuario en el sistema
        /// </summary>
        /// <param name="registroDto">Datos del usuario a registrar</param>
        /// <returns>Respuesta con información del usuario registrado</returns>
        [HttpPost("registro")]
        [ServiceFilter(typeof(AccesoAttribute))]
        [JwtAuthorization]
        [ServiceFilter(typeof(ValidarModeloAttribute))]
        public async Task<ActionResult<RespuestaDto>> Registro([FromBody] RegistroDto registroDto)
        {
            try
            {
                // Registrar usuario
                var resultado = await _authRepository.RegistrarAsync(registroDto);

                if (resultado.Exito)
                {
                    return Ok(resultado);
                }
                else
                {
                    return BadRequest(resultado);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, RespuestaDto.ErrorInterno(
                    $"Error interno del servidor: {ex.Message}"));
            }
        }

        /// <summary>
        /// Valida un token JWT
        /// </summary>
        /// <returns>Información del usuario si el token es válido</returns>
        [HttpGet("validar-token")]
        [ServiceFilter(typeof(AccesoAttribute))]
        [JwtAuthorization]
        public async Task<ActionResult<RespuestaDto>> ValidarToken()
        {
            try
            {
                // Obtener ID del usuario desde el token JWT
                var userIdClaim = HttpContext.User.FindFirst("id")?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
                {
                    return Unauthorized(RespuestaDto.ParametrosIncorrectos(
                        "Token inválido",
                        "No se pudo obtener el ID del usuario del token"));
                }

                // Obtener información del usuario
                var usuario = await _authRepository.ObtenerUsuarioPorIdAsync(userId);

                if (usuario == null)
                {
                    return NotFound(RespuestaDto.ParametrosIncorrectos(
                        "Usuario no encontrado",
                        "El usuario especificado no existe en el sistema"));
                }

                return Ok(RespuestaDto.Exitoso(
                    "Token válido",
                    "El token es válido y el usuario existe",
                    usuario));
            }
            catch (Exception ex)
            {
                return StatusCode(500, RespuestaDto.ErrorInterno(
                    $"Error interno del servidor: {ex.Message}"));
            }
        }

        /// <summary>
        /// Endpoint para verificar el estado de la API
        /// </summary>
        /// <returns>Estado de la API</returns>
        [HttpGet("health")]
        [ServiceFilter(typeof(AccesoAttribute))]
        public ActionResult<RespuestaDto> Health()
        {
            try
            {
                return Ok(RespuestaDto.Exitoso(
                    "API funcionando",
                    "La API de autenticación está operativa",
                    new
                    {
                        Timestamp = DateTime.UtcNow,
                        Version = "1.0.0",
                        Status = "Healthy"
                    }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, RespuestaDto.ErrorInterno(
                    $"Error en health check: {ex.Message}"));
            }
        }
    }
}