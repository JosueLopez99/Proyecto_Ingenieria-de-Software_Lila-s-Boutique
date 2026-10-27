using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorApp1.Models
{
    public class VariantePrenda
    {
        [Key]
        public int IdVariante { get; set; }
        [Required, MaxLength(50)]
        public string CodigoPrenda { get; set; } = null!;
        [Required, MaxLength(10)]
        public string Talla { get; set; } = null!;
        [Required, MaxLength(50)]
        public string Color { get; set; } = null!;
        public int StockFisicoActual { get; set; } = 0;
        public int LimiteAlerta { get; set; } = 10;

        [ForeignKey("CodigoPrenda")]
        public virtual Prenda Prenda { get; set; } = null!;
    }
}
