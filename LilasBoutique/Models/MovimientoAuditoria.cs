using System;
using System.Collections.Generic;

namespace LilasBoutique.Models
{
    public class MovimientoAuditoria
    {
        public int IdMovimiento { get; set; }
        public DateTime FechaHora { get; set; }
        public string TipoOperacion { get; set; } = null!;
        public int CantidadAfectada { get; set; }
        public string? Justificacion { get; set; }
        public int UsuarioId { get; set; }
        public int VarianteId { get; set; }

        public Usuario Usuario { get; set; } = null!;
        public VariantePrenda Variante { get; set; } = null!;
    }
}
