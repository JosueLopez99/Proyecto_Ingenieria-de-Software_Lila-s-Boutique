using System;
using System.Collections.Generic;

namespace LilasBoutique.Models
{
    public class VariantePrenda
    {
        public int IdVariante { get; set; }
        public int PrendaId { get; set; }
        public int TallaId { get; set; }
        public int ColorId { get; set; }
        public int StockFisicoActual { get; set; }
        public decimal Precio { get; set; }
        public int LimiteAlerta { get; set; }

        public Prenda Prenda { get; set; } = null!;
        public Talla Talla { get; set; } = null!;
        public Color Color { get; set; } = null!;
        public ICollection<MovimientoAuditoria> Movimientos { get; set; } = new List<MovimientoAuditoria>();
    }
}
