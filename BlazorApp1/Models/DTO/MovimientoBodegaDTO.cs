using System.ComponentModel.DataAnnotations;

namespace BlazorApp1.Models.DTO
{
    public class MovimientoBodegaDTO
    {
        [Required]
        public int IdVariante { get; set; }
        [Required]
        public string TipoOperacion { get; set; } = string.Empty;
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Cantidad { get; set; }
        public string? Justificacion { get; set; }
    }
}
