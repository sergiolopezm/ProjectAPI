using System.ComponentModel.DataAnnotations;

namespace ProjectAPI.Shared.AuthDTO
{
    public class RegistroDto
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [StringLength(100)]
        public string NombreUsuario { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(250)]
        public string Contraseña { get; set; } = null!;

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100)]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(100)]
        public string Apellido { get; set; } = null!;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [StringLength(100)]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "El rol es requerido")]
        public int RolId { get; set; }
    }
}
