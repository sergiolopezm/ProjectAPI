using Business.Contracts;
using DataAccess;
using DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using ProjectAPI.Shared.CustomerDTO;
using ProjectAPI.Shared.GeneralDTO;
using ProjectAPI.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Business
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly BaseModel<Customer> _baseModel;
        private readonly JujuTestContext _context;

        public CustomerRepository(BaseModel<Customer> baseModel, JujuTestContext context)
        {
            _baseModel = baseModel;
            _context = context;
        }

        public async Task<List<CustomerDto>> ObtenerTodosAsync()
        {
            var customers = await _baseModel.GetAll.ToListAsync();
            return customers.Select(c => Mapping.Convertir<Customer, CustomerDto>(c)).ToList();
        }

        public async Task<CustomerDto?> ObtenerPorIdAsync(int id)
        {
            var customer = await _context.Customer.FindAsync(id);
            return customer != null ? Mapping.Convertir<Customer, CustomerDto>(customer) : null;
        }

        public async Task<RespuestaDto> CrearAsync(CustomerCreateDto customerDto)
        {
            try
            {
                var customer = Mapping.Convertir<CustomerCreateDto, Customer>(customerDto);
                var resultado = _baseModel.Create(customer);

                return RespuestaDto.Exitoso(
                    "Customer creado",
                    $"Customer '{resultado.Name}' creado exitosamente",
                    Mapping.Convertir<Customer, CustomerDto>(resultado));
            }
            catch (Exception ex)
            {
                return RespuestaDto.ErrorInterno($"Error al crear customer: {ex.Message}");
            }
        }

        public async Task<RespuestaDto> ActualizarAsync(int id, CustomerUpdateDto customerDto)
        {
            try
            {
                var customerExistente = _baseModel.FindById(id);
                if (customerExistente == null)
                    return RespuestaDto.NoEncontrado("Customer");

                var customerActualizado = Mapping.Convertir<CustomerUpdateDto, Customer>(customerDto);
                var resultado = _baseModel.Update(customerActualizado, customerExistente, out bool changed);

                if (!changed)
                    return RespuestaDto.ParametrosIncorrectos("Sin cambios", "No se detectaron cambios en el customer");

                return RespuestaDto.Exitoso(
                    "Customer actualizado",
                    $"Customer '{resultado.Name}' actualizado exitosamente",
                    Mapping.Convertir<Customer, CustomerDto>(resultado));
            }
            catch (Exception ex)
            {
                return RespuestaDto.ErrorInterno($"Error al actualizar customer: {ex.Message}");
            }
        }

        public async Task<RespuestaDto> EliminarAsync(int id)
        {
            try
            {
                var customer = _baseModel.FindById(id);
                if (customer == null)
                    return RespuestaDto.NoEncontrado("Customer");

                _baseModel.Delete(customer);
                return RespuestaDto.Exitoso("Customer eliminado", $"Customer '{customer.Name}' eliminado exitosamente");
            }
            catch (Exception ex)
            {
                return RespuestaDto.ErrorInterno($"Error al eliminar customer: {ex.Message}");
            }
        }

        public async Task<bool> ExisteAsync(int id)
        {
            return await _context.Customer.AnyAsync(c => c.CustomerId == id);
        }

        public async Task<PaginacionDto<CustomerDto>> ObtenerPaginadoAsync(int pagina, int elementosPorPagina, string? busqueda = null)
        {
            var query = _context.Customer.AsQueryable();

            if (!string.IsNullOrEmpty(busqueda))
                query = query.Where(c => c.Name!.Contains(busqueda));

            var total = await query.CountAsync();
            var customers = await query
                .Skip((pagina - 1) * elementosPorPagina)
                .Take(elementosPorPagina)
                .ToListAsync();

            return new PaginacionDto<CustomerDto>
            {
                Pagina = pagina,
                ElementosPorPagina = elementosPorPagina,
                TotalRegistros = total,
                TotalPaginas = (int)Math.Ceiling(total / (double)elementosPorPagina),
                Lista = customers.Select(c => Mapping.Convertir<Customer, CustomerDto>(c)).ToList()
            };
        }
    }
}
