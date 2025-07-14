using DataAccess.Data;
using ProjectAPI.Shared.GeneralDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.Contracts
{
    public interface IPostRepository
    {
        Task<List<Post>> ObtenerTodosAsync();
        Task<Post?> ObtenerPorIdAsync(int id);
        Task<RespuestaDto> CrearAsync(Post post);
        Task<RespuestaDto> ActualizarAsync(int id, Post post);
        Task<RespuestaDto> EliminarAsync(int id);
        Task<List<Post>> BuscarAsync(string termino);
        Task<List<Post>> ObtenerPorUsuarioAsync(int userId);
        Task<RespuestaDto> CrearMultiplesAsync(List<Post> posts);
    }
}
