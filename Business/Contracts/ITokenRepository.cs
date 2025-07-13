using DataAccess.Data;
using System.Threading.Tasks;

namespace Business.Contracts
{
    public interface ITokenRepository
    {
        Task<string> GenerarTokenAsync(Usuarios usuario, string ip);
        Task<bool> ValidarTokenAsync(string token);
        Task ExpiraTokenAsync(string token);
    }
}
