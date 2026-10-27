using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorApp1.Models
{
    public class MovimientoAuditoria
    {
        [Key]
        public int IdMovimiento { get; set; }
        public DateTime FechaHora { get; set; } = DateTime.Now;
        [Required, MaxLength(50)]
        public string TipoOperacion { get; set; } = null!; // RECEPCION_PROVEEDOR, VENTA_CLIENTE, MERMA_DAÑO
        public int CantidadAfectada { get; set; }
        [MaxLength(255)]
        public string? Justificacion { get; set; }

        public int IdUsuario { get; set; }
        public int IdVariante { get; set; }

        [ForeignKey("IdUsuario")]
        public virtual Usuario Usuario { get; set; } = null!;
        [ForeignKey("IdVariante")]
        public virtual VariantePrenda VariantePrenda { get; set; } = null!;
    }
}
      

