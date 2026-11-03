using LilasBoutique.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace LilasBoutique.Data
{
    public class LilaBoutiqueContext : DbContext
    {
        public LilaBoutiqueContext(DbContextOptions<LilaBoutiqueContext> options) : base(options) { }

        public DbSet<Categoria> Categoria { get; set; }
        public DbSet<Talla> Talla { get; set; }
        public DbSet<Color> Color { get; set; }
        public DbSet<Rol> Rol { get; set; }
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<Prenda> Prenda { get; set; }
        public DbSet<VariantePrenda> VariantePrenda { get; set; }
        public DbSet<MovimientoAuditoria> MovimientoAuditoria { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==========================================
            // Mapeo de Catálogos (Categoría, Talla, Color)
            // ==========================================
            modelBuilder.Entity<Categoria>(entity =>
            {
                entity.ToTable("Categoria");
                entity.HasKey(e => e.IdCategoria);
                entity.Property(e => e.NombreCategoria).HasMaxLength(50).IsRequired();
                entity.HasIndex(e => e.NombreCategoria).IsUnique();
            });

            modelBuilder.Entity<Talla>(entity =>
            {
                entity.ToTable("Talla");
                entity.HasKey(e => e.IdTalla);
                entity.Property(e => e.NombreTalla).HasMaxLength(10).IsRequired();
                entity.HasIndex(e => e.NombreTalla).IsUnique();
            });

            modelBuilder.Entity<Color>(entity =>
            {
                entity.ToTable("Color");
                entity.HasKey(e => e.IdColor);
                entity.Property(e => e.NombreColor).HasMaxLength(50).IsRequired();
                entity.HasIndex(e => e.NombreColor).IsUnique();
            });

            // ==========================================
            // Mapeo de Seguridad (Rol y Usuario)
            // ==========================================
            modelBuilder.Entity<Rol>(entity =>
            {
                entity.ToTable("Rol");
                entity.HasKey(e => e.IdRol);
                entity.Property(e => e.NombreRol).HasMaxLength(50).IsRequired();
            });

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuario");
                entity.HasKey(e => e.IdUsuario);
                entity.Property(e => e.Correo).HasMaxLength(255).IsRequired();
                entity.HasIndex(e => e.Correo).IsUnique();
                entity.Property(e => e.HashCredencial).HasMaxLength(255).IsRequired();
                entity.Property(e => e.EstadoActivo).HasDefaultValue(true);

                entity.HasOne(d => d.Rol)
                      .WithMany(p => p.Usuarios)
                      .HasForeignKey(d => d.RolId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================================
            // Mapeo de Inventario (Prenda y Variante)
            // ==========================================
            modelBuilder.Entity<Prenda>(entity =>
            {
                entity.ToTable("Prenda");
                entity.HasKey(e => e.IdPrenda);
                entity.Property(e => e.CodigoIdentificador).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Marca).HasMaxLength(100).IsRequired();
                entity.Property(e => e.DescripcionBreve).HasMaxLength(255);

                entity.HasOne(d => d.Categoria)
                      .WithMany(p => p.Prendas)
                      .HasForeignKey(d => d.CategoriaId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<VariantePrenda>(entity =>
            {
                // CORRECCIÓN: Check constraint dentro de ToTable respetando la regla (StockFisicoActual >= 0)[cite: 2, 3]
                entity.ToTable("VariantePrenda", t => t.HasCheckConstraint("CHK_Stock_Fisico", "StockFisicoActual >= 0"));

                entity.HasKey(e => e.IdVariante);
                entity.Property(e => e.StockFisicoActual).HasDefaultValue(0);
                entity.Property(e => e.Precio).HasColumnType("DECIMAL(18,2)").IsRequired();
                entity.Property(e => e.LimiteAlerta).HasDefaultValue(10);

                entity.HasOne(d => d.Prenda)
                      .WithMany(p => p.Variantes)
                      .HasForeignKey(d => d.PrendaId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Talla)
                      .WithMany(p => p.Variantes)
                      .HasForeignKey(d => d.TallaId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Color)
                      .WithMany(p => p.Variantes)
                      .HasForeignKey(d => d.ColorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================================
            // Mapeo de Auditoría
            // ==========================================
            modelBuilder.Entity<MovimientoAuditoria>(entity =>
            {
                // CORRECCIÓN: Check constraint dentro de ToTable validando los tipos de operación[cite: 2, 3]
                entity.ToTable("MovimientoAuditoria", t => t.HasCheckConstraint("CHK_Tipo_Operacion", "TipoOperacion IN ('RECEPCION_PROVEEDOR', 'VENTA_CLIENTE', 'MERMA_DAÑO')"));

                entity.HasKey(e => e.IdMovimiento);
                entity.Property(e => e.FechaHora).HasColumnType("DATETIME2").HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.TipoOperacion).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Justificacion).HasMaxLength(255);

                entity.HasOne(d => d.Usuario)
                      .WithMany(p => p.Movimientos)
                      .HasForeignKey(d => d.UsuarioId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Variante)
                      .WithMany(p => p.Movimientos)
                      .HasForeignKey(d => d.VarianteId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}