using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Data
{
    public partial class Tokens
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(1000)]
        public string Token { get; set; } = null!;

        public Guid UsuarioId { get; set; }

        [Required]
        [StringLength(45)]
        public string Ip { get; set; } = null!;

        public DateTime FechaCreacion { get; set; }

        public DateTime FechaExpiracion { get; set; }

        public virtual Usuarios Usuario { get; set; } = null!;
    }
}
