using System;
using System.Collections.Generic;

namespace LilasBoutique.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string Correo { get; set; } = null!;
        public string HashCredencial { get; set; } = null!;
        public bool EstadoActivo { get; set; }
        public int RolId { get; set; }

        public Rol Rol { get; set; } = null!;
        public ICollection<MovimientoAuditoria> Movimientos { get; set; } = new List<MovimientoAuditoria>();
    }
}
