using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Data
{
    public partial class Accesos
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Sitio { get; set; } = null!;

        [Required]
        [StringLength(250)]
        public string Contraseña { get; set; } = null!;

        public DateTime FechaCreacion { get; set; }

        public bool Activo { get; set; }
    }
}
