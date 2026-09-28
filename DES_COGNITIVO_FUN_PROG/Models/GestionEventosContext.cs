using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DES_COGNITIVO_FUN_PROG.Models;

public partial class GestionEventosContext : DbContext
{
    public GestionEventosContext()
    {
    }

    public GestionEventosContext(DbContextOptions<GestionEventosContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Administrador> Administradors { get; set; } //estos son los controladores espejo de las tablas de la base de datos

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<EstadoEvento> EstadoEventos { get; set; }

    public virtual DbSet<Evento> Eventos { get; set; }

    public virtual DbSet<Invitado> Invitados { get; set; }

    public virtual DbSet<Mesa> Mesas { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=GestionEventos;Trusted_Connection=True; TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Administrador>(entity =>
        {
            entity.HasKey(e => e.IdAdmin);

            entity.ToTable("Administrador");

            entity.Property(e => e.ContraseñaHash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Usuario)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.IdCliente);

            entity.ToTable("Cliente");

            entity.Property(e => e.Correo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NumeroIdentidad)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("numero_identidad");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<EstadoEvento>(entity =>
        {
            entity.HasKey(e => e.IdEstado);

            entity.ToTable("Estado_Evento");

            entity.Property(e => e.EstadoEvento1)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("EstadoEvento");
        });

        modelBuilder.Entity<Evento>(entity =>
        {
            entity.HasKey(e => e.IdEvento);

            entity.ToTable("Evento");

            entity.Property(e => e.CodigoEvento)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Lugar)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Presupuesto).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Eventos)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Evento_Cliente");

            entity.HasOne(d => d.IdEstadoNavigation).WithMany(p => p.Eventos)
                .HasForeignKey(d => d.IdEstado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Evento_Estado");
        });

        modelBuilder.Entity<Invitado>(entity =>
        {
            entity.HasKey(e => e.IdInvitado);

            entity.ToTable("Invitado");

            entity.Property(e => e.Dni)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("DNI");
            entity.Property(e => e.Nombres)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.NumAcompañantes).HasDefaultValue(0);

            entity.HasOne(d => d.IdEventoNavigation).WithMany(p => p.Invitados)
                .HasForeignKey(d => d.IdEvento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Invitado_Evento");

            entity.HasOne(d => d.IdMesaNavigation).WithMany(p => p.Invitados)
                .HasForeignKey(d => d.IdMesa)
                .HasConstraintName("FK_Invitado_Mesa");
        });

        modelBuilder.Entity<Mesa>(entity =>
        {
            entity.HasKey(e => e.IdMesa);

            entity.ToTable("Mesa");

            entity.HasOne(d => d.IdEventoNavigation).WithMany(p => p.Mesas)
                .HasForeignKey(d => d.IdEvento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Mesa_Evento");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
