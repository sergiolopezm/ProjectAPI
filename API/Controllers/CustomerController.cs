using API.Attributes;
using Business;
using Business.Contracts;
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
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerController(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        [HttpGet]
        [ServiceFilter(typeof(LogAttribute))]
        public async Task<ActionResult<RespuestaDto>> GetAll()
        {
            var customers = await _customerRepository.ObtenerTodosAsync();
            return Ok(RespuestaDto.Exitoso("Customers obtenidos", $"Se obtuvieron {customers.Count} customers", customers));
        }

        [HttpGet("{id}")]
        [ServiceFilter(typeof(LogAttribute))]
        public async Task<ActionResult<RespuestaDto>> GetById(int id)
        {
            var customer = await _customerRepository.ObtenerPorIdAsync(id);
            if (customer == null)
                return NotFound(RespuestaDto.NoEncontrado("Customer"));

            return Ok(RespuestaDto.Exitoso("Customer encontrado", "Customer obtenido exitosamente", customer));
        }

        [HttpPost]
        [ServiceFilter(typeof(ValidarModeloAttribute))]
        [ServiceFilter(typeof(LogAttribute))]
        public async Task<ActionResult<RespuestaDto>> Create([FromBody] CustomerCreateDto customerDto)
        {
            var resultado = await _customerRepository.CrearAsync(customerDto);
            if (resultado.Exito)
                return CreatedAtAction(nameof(GetById), new { id = ((CustomerDto)resultado.Resultado!).CustomerId }, resultado);

            return BadRequest(resultado);
        }

        [HttpPut("{id}")]
        [ServiceFilter(typeof(ValidarModeloAttribute))]
        [ServiceFilter(typeof(LogAttribute))]
        public async Task<ActionResult<RespuestaDto>> Update(int id, [FromBody] CustomerUpdateDto customerDto)
        {
            if (customerDto.CustomerId != id)
                return BadRequest(RespuestaDto.ParametrosIncorrectos("ID no coincide", "El ID del customer no coincide"));

            var resultado = await _customerRepository.ActualizarAsync(id, customerDto);
            return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
        }

        [HttpDelete("{id}")]
        [ServiceFilter(typeof(LogAttribute))]
        public async Task<ActionResult<RespuestaDto>> Delete(int id)
        {
            var resultado = await _customerRepository.EliminarAsync(id);
            return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
        }

        [HttpGet("paginado")]
        [ServiceFilter(typeof(LogAttribute))]
        public async Task<ActionResult<RespuestaDto>> GetPaginado([FromQuery] int pagina = 1, [FromQuery] int elementos = 10, [FromQuery] string? busqueda = null)
        {
            var resultado = await _customerRepository.ObtenerPaginadoAsync(pagina, elementos, busqueda);
            return Ok(RespuestaDto.Exitoso("Customers paginados", $"Página {pagina} obtenida", resultado));
        }
    }
}
