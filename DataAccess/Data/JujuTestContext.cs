using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DataAccess.Data
{
    public partial class JujuTestContext : DbContext
    {
        public JujuTestContext()
        {
        }

        public JujuTestContext(DbContextOptions<JujuTestContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Customer> Customer { get; set; }
        public virtual DbSet<Logs> Logs { get; set; }
        public virtual DbSet<Post> Post { get; set; }
        public virtual DbSet<Accesos> Accesos { get; set; }
        public virtual DbSet<Roles> Roles { get; set; }
        public virtual DbSet<Usuarios> Usuarios { get; set; }
        public virtual DbSet<Tokens> Tokens { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuraciones existentes
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(500);
            });

            modelBuilder.Entity<Logs>(entity =>
            {
                entity.Property(e => e.TimeStamp).HasColumnType("datetime");
            });

            modelBuilder.Entity<Post>(entity =>
            {
                entity.Property(e => e.Body).HasMaxLength(500);
                entity.Property(e => e.Category).HasMaxLength(500);
                entity.Property(e => e.Title).HasMaxLength(500);
            });

            // NUEVAS configuraciones
            modelBuilder.Entity<Usuarios>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasDefaultValueSql("NEWID()");
                entity.HasIndex(e => e.NombreUsuario).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();

                entity.HasOne(d => d.Rol)
                    .WithMany(p => p.Usuarios)
                    .HasForeignKey(d => d.RolId);
            });

            modelBuilder.Entity<Tokens>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasDefaultValueSql("NEWID()");

                entity.HasOne(d => d.Usuario)
                    .WithMany(p => p.Tokens)
                    .HasForeignKey(d => d.UsuarioId);
            });

            modelBuilder.Entity<Roles>(entity =>
            {
                entity.HasIndex(e => e.Nombre).IsUnique();
            });

            modelBuilder.Entity<Accesos>(entity =>
            {
                entity.Property(e => e.FechaCreacion).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.Activo).HasDefaultValue(true);
            });
        }
    }
}
