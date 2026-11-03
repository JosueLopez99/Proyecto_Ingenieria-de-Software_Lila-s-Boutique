using System;
using System.Collections.Generic;

namespace LilasBoutique.Models
{
    public class Prenda
    {
        public int IdPrenda { get; set; }
        public string CodigoIdentificador { get; set; } = null!;
        public string Marca { get; set; } = null!;
        public string? DescripcionBreve { get; set; }
        public int CategoriaId { get; set; }

        public Categoria Categoria { get; set; } = null!;
        public ICollection<VariantePrenda> Variantes { get; set; } = new List<VariantePrenda>();
    }
}
