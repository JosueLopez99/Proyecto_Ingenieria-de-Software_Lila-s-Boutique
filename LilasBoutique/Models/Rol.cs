using System;
using System.Collections.Generic;
namespace LilasBoutique.Models
{
    public class Rol
    {
        public int IdRol { get; set; }
        public string NombreRol { get; set; } = null!;
        public int NivelPrivilegio { get; set; }
        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}
