using Microsoft.EntityFrameworkCore;
using BlazorApp1.Models;

namespace BlazorApp1.Data

{
    public class LilaBoutiqueContext : DbContext
    {
        public LilaBoutiqueContext(DbContextOptions<LilaBoutiqueContext> options)
            : base(options)
        {
        }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Prenda> Prendas { get; set; }
        public DbSet<VariantePrenda> VariantesPrenda { get; set; }
        public DbSet<MovimientoAuditoria> MovimientosAuditoria { get; set; }
    }
}
