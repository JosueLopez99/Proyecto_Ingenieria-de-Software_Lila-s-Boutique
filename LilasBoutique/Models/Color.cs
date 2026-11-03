using System;
using System.Collections.Generic;

namespace LilasBoutique.Models
{

    public class Color
    {
        public int IdColor { get; set; }
        public string NombreColor { get; set; } = null!;
        public ICollection<VariantePrenda> Variantes { get; set; } = new List<VariantePrenda>();
    }
}