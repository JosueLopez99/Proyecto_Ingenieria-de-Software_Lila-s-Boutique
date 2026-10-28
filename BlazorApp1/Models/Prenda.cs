using System.ComponentModel.DataAnnotations;

namespace BlazorApp1.Models
{
    public class Prenda
    {
        [Key, MaxLength(50)]
        public string CodigoIdentificador { get; set; } = null!;
        [Required, MaxLength(100)]
        public string Marca { get; set; } = null!;
        [MaxLength(255)]
        public string? DescripcionBreve { get; set; }
        [Required, MaxLength(50)]
        public string Categoria { get; set; } = null!;

        public virtual ICollection<VariantePrenda> Variantes { get; set; } = new List<VariantePrenda>();
    }
}
