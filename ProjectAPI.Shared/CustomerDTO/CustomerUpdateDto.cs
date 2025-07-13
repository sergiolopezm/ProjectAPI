using System.ComponentModel.DataAnnotations;

namespace ProjectAPI.Shared.CustomerDTO
{
    public class CustomerUpdateDto
    {
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(500, ErrorMessage = "El nombre no puede exceder 500 caracteres")]
        public string Name { get; set; } = null!;
    }
}
