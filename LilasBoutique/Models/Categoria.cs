using System;
using System.Collections.Generic;

namespace LilasBoutique.Models
{
    public class Categoria
    {
        public int IdCategoria { get; set; }
        public string NombreCategoria { get; set; } = null!;
        public ICollection<Prenda> Prendas { get; set; } = new List<Prenda>();
    }
}