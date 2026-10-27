using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorApp1.Models
{
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }
        [Required, MaxLength(255)]
        public string Correo { get; set; } = null!;
        [Required, MaxLength(255)]
        public string HashCredencial { get; set; } = null!; // Cumplimiento Ley 787
        public bool EstadoActivo { get; set; } = true;

        public int IdRol { get; set; }
        [ForeignKey("IdRol")]
        public virtual Rol Rol { get; set; } = null!;
    }
}
