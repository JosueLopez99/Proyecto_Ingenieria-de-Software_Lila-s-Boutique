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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- CORRECCIÓN: Forzar el mapeo exacto a las tablas en singular de SQL Server ---
            modelBuilder.Entity<Rol>().ToTable("Rol");
            modelBuilder.Entity<Usuario>().ToTable("Usuario");
            modelBuilder.Entity<Prenda>().ToTable("Prenda");
            modelBuilder.Entity<MovimientoAuditoria>().ToTable("MovimientoAuditoria");

            // Regla de negocio combinada con el nombre correcto de la tabla
            modelBuilder.Entity<VariantePrenda>()
                .ToTable("VariantePrenda", tb => tb.HasCheckConstraint("CHK_Stock_Fisico", "StockFisicoActual >= 0"));

            // 1. Sembrar el Rol
            modelBuilder.Entity<Rol>().HasData(
                new Rol { IdRol = 1, NombreRol = "Administradora", NivelPrivilegio = 1 }
            );

            // 2. Sembrar el Usuario con la contraseña encriptada
            modelBuilder.Entity<Usuario>().HasData(
                new Usuario
                {
                    IdUsuario = 1,
                    Correo = "admin@lilasboutique.com",
                    // Hasheamos "admin123" automáticamente al compilar
                    HashCredencial = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    EstadoActivo = true,
                    IdRol = 1
                }
            );
        }
    }
}