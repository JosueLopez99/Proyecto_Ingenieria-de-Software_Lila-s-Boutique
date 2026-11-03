using System;
using System.Collections.Generic;

namespace LilasBoutique.Models
{

    public class Talla
    {
        public int IdTalla { get; set; }
        public string NombreTalla { get; set; } = null!;
        // Nueva relación con Categoría
        public int CategoriaId { get; set; }

        // Propiedad de navegación (opcional pero recomendada)
        public Categoria Categoria { get; set; }
        public ICollection<VariantePrenda> Variantes { get; set; } = new List<VariantePrenda>();
    }
}