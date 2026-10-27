using System.ComponentModel.DataAnnotations;

namespace BlazorApp1.Models
{
    public class Rol
    {
        [Key]
        public int IdRol { get; set; }
        [Required, MaxLength(50)]
        public string NombreRol { get; set; } = null!;
        public int NivelPrivilegio { get; set; }
    }
}


