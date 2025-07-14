using API.Attributes;
using Business;
using Business.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjectAPI.Shared.CustomerDTO;
using ProjectAPI.Shared.GeneralDTO;
using System;
using System.Linq;
using System.Threading.Tasks;
using CustomerEntity = DataAccess.Data.Customer;

namespace API.Controllers.Customer
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    [ServiceFilter(typeof(AccesoAttribute))]
    [ServiceFilter(typeof(LogAttribute))]
    [ServiceFilter(typeof(ExceptionAttribute))]
    [ProducesResponseType(typeof(RespuestaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespuestaDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(RespuestaDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespuestaDto), StatusCodes.Status500InternalServerError)]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerController(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        /// <summary>
        /// Obtiene todos los customers
        /// </summary>
        /// <returns>Lista de todos los customers</returns>
        [HttpGet]
        public async Task<ActionResult<RespuestaDto>> GetAll()
        {
            try
            {
                var customers = await _customerRepository.ObtenerTodosAsync();
                return Ok(RespuestaDto.Exitoso(
                    "Customers obtenidos",
                    $"Se obtuvieron {customers.Count} customers",
                    customers));
            }
            catch (Exception ex)
            {
                return StatusCode(500, RespuestaDto.ErrorInterno(
                    $"Error al obtener customers: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtiene un customer por ID
        /// </summary>
        /// <param name="id">ID del customer</param>
        /// <returns>Customer encontrado</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<RespuestaDto>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(RespuestaDto.ParametrosIncorrectos(
                        "ID inválido",
                        "El ID debe ser un número mayor a 0"));
                }

                var customer = await _customerRepository.ObtenerPorIdAsync(id);

                if (customer == null)
                {
                    return NotFound(RespuestaDto.ParametrosIncorrectos(
                        "Customer no encontrado",
                        $"No se encontró el customer con ID {id}"));
                }

                return Ok(RespuestaDto.Exitoso(
                    "Customer encontrado",
                    "Customer obtenido exitosamente",
                    customer));
            }
            catch (Exception ex)
            {
                return StatusCode(500, RespuestaDto.ErrorInterno(
                    $"Error al obtener customer: {ex.Message}"));
            }
        }

        /// <summary>
        /// Crea un nuevo customer
        /// </summary>
        /// <param name="customerDto">Datos del customer a crear</param>
        /// <returns>Customer creado</returns>
        [HttpPost]
        [ServiceFilter(typeof(ValidarModeloAttribute))]
        public async Task<ActionResult<RespuestaDto>> Create([FromBody] CustomerCreateDto customerDto)
        {
            try
            {
                var resultado = await _customerRepository.CrearAsync(customerDto);

                if (resultado.Exito)
                {
                    var customerCreado = (CustomerDto)resultado.Resultado!;
                    return CreatedAtAction(
                        nameof(GetById),
                        new { id = customerCreado.CustomerId },
                        resultado);
                }

                return BadRequest(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, RespuestaDto.ErrorInterno(
                    $"Error al crear customer: {ex.Message}"));
            }
        }

        /// <summary>
        /// Actualiza un customer existente
        /// </summary>
        /// <param name="id">ID del customer a actualizar</param>
        /// <param name="customerDto">Datos del customer actualizados</param>
        /// <returns>Customer actualizado</returns>
        [HttpPut("{id}")]
        [ServiceFilter(typeof(ValidarModeloAttribute))]
        public async Task<ActionResult<RespuestaDto>> Update(int id, [FromBody] CustomerUpdateDto customerDto)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(RespuestaDto.ParametrosIncorrectos(
                        "ID inválido",
                        "El ID debe ser un número mayor a 0"));
                }

                if (customerDto.CustomerId != id)
                {
                    return BadRequest(RespuestaDto.ParametrosIncorrectos(
                        "ID no coincide",
                        "El ID del customer no coincide con el ID de la URL"));
                }

                var resultado = await _customerRepository.ActualizarAsync(id, customerDto);

                if (resultado.Exito)
                {
                    return Ok(resultado);
                }

                return BadRequest(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, RespuestaDto.ErrorInterno(
                    $"Error al actualizar customer: {ex.Message}"));
            }
        }

        /// <summary>
        /// Elimina un customer
        /// </summary>
        /// <param name="id">ID del customer a eliminar</param>
        /// <returns>Confirmación de eliminación</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult<RespuestaDto>> Delete(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(RespuestaDto.ParametrosIncorrectos(
                        "ID inválido",
                        "El ID debe ser un número mayor a 0"));
                }

                var resultado = await _customerRepository.EliminarAsync(id);

                if (resultado.Exito)
                {
                    return Ok(resultado);
                }

                return NotFound(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, RespuestaDto.ErrorInterno(
                    $"Error al eliminar customer: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtiene customers paginados con búsqueda opcional
        /// </summary>
        /// <param name="pagina">Número de página</param>
        /// <param name="elementosPorPagina">Elementos por página</param>
        /// <param name="busqueda">Término de búsqueda opcional</param>
        /// <returns>Lista paginada de customers</returns>
        [HttpGet("paginado")]
        public async Task<ActionResult<RespuestaDto>> ObtenerPaginado(
            [FromQuery] int pagina = 1,
            [FromQuery] int elementosPorPagina = 10,
            [FromQuery] string? busqueda = null)
        {
            try
            {
                if (pagina <= 0)
                {
                    return BadRequest(RespuestaDto.ParametrosIncorrectos(
                        "Página inválida",
                        "El número de página debe ser mayor a 0"));
                }

                if (elementosPorPagina <= 0 || elementosPorPagina > 100)
                {
                    return BadRequest(RespuestaDto.ParametrosIncorrectos(
                        "Elementos por página inválido",
                        "Los elementos por página deben estar entre 1 y 100"));
                }

                var resultado = await _customerRepository.ObtenerPaginadoAsync(pagina, elementosPorPagina, busqueda);

                return Ok(RespuestaDto.Exitoso(
                    "Customers obtenidos",
                    $"Se encontraron {resultado.Lista?.Count ?? 0} customers de {resultado.TotalRegistros} total",
                    resultado));
            }
            catch (Exception ex)
            {
                return StatusCode(500, RespuestaDto.ErrorInterno(
                    $"Error al obtener customers paginados: {ex.Message}"));
            }
        }
    }
}