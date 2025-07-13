using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Data
{
    public partial class Usuarios
    {
        public Usuarios()
        {
            Tokens = new HashSet<Tokens>();
        }

        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string NombreUsuario { get; set; } = null!;

        [Required]
        [StringLength(250)]
        public string Contraseña { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Apellido { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Email { get; set; } = null!;

        public int RolId { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaCreacion { get; set; }

        public DateTime? FechaUltimoAcceso { get; set; }

        public virtual Roles Rol { get; set; } = null!;
        public virtual ICollection<Tokens> Tokens { get; set; }
    }
}
