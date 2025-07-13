using System.ComponentModel.DataAnnotations;

namespace ProjectAPI.Shared.AuthDTO
{
    public class LoginDto
    {
        [Required(ErrorMessage = "El sitio es requerido")]
        public string Sitio { get; set; } = null!;

        [Required(ErrorMessage = "La clave es requerida")]
        public string Clave { get; set; } = null!;

        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        public string NombreUsuario { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es requerida")]
        public string Contraseña { get; set; } = null!;

        public string? Ip { get; set; }
    }
}
