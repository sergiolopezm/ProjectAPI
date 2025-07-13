using ProjectAPI.Shared.CustomerDTO;
using ProjectAPI.Shared.GeneralDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.Contracts
{
    public interface ICustomerRepository
    {
        Task<List<CustomerDto>> ObtenerTodosAsync();
        Task<CustomerDto?> ObtenerPorIdAsync(int id);
        Task<RespuestaDto> CrearAsync(CustomerCreateDto customerDto);
        Task<RespuestaDto> ActualizarAsync(int id, CustomerUpdateDto customerDto);
        Task<RespuestaDto> EliminarAsync(int id);
        Task<bool> ExisteAsync(int id);
        Task<PaginacionDto<CustomerDto>> ObtenerPaginadoAsync(int pagina, int elementosPorPagina, string? busqueda = null);
    }
}
