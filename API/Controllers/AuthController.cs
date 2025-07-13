using API.Attributes;
using Business.Contracts;
using Microsoft.AspNetCore.Mvc;
using ProjectAPI.Shared.AuthDTO;
using ProjectAPI.Shared.GeneralDTO;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;

        public AuthController(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        [HttpPost("login")]
        [ServiceFilter(typeof(ValidarModeloAttribute))]
        [ServiceFilter(typeof(LogAttribute))]
        public async Task<ActionResult<RespuestaDto>> Login([FromBody] LoginDto loginDto)
        {
            var resultado = await _authRepository.AutenticarAsync(loginDto);
            return resultado.Exito ? Ok(resultado) : Unauthorized(resultado);
        }

        [HttpPost("registro")]
        [JwtAuthorization]
        [ServiceFilter(typeof(ValidarModeloAttribute))]
        [ServiceFilter(typeof(LogAttribute))]
        public async Task<ActionResult<RespuestaDto>> Registro([FromBody] RegistroDto registroDto)
        {
            var resultado = await _authRepository.RegistrarAsync(registroDto);
            return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
        }
    }
}
