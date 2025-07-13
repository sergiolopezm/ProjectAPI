using ProjectAPI.Shared.AuthDTO;
using ProjectAPI.Shared.GeneralDTO;
using System;
using System.Threading.Tasks;

namespace Business.Contracts
{
    public interface IAuthRepository
    {
        Task<RespuestaDto> AutenticarAsync(LoginDto loginDto);
        Task<RespuestaDto> RegistrarAsync(RegistroDto registroDto);
        Task<bool> ValidarAccesoAsync(string sitio, string clave);
        Task<UsuarioDto?> ObtenerUsuarioPorIdAsync(Guid id);
    }
}
