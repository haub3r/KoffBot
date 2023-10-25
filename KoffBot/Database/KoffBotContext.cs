using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace KoffBot.Database;

public partial class KoffBotContext : DbContext
{
    public KoffBotContext()
    {
    }

    public KoffBotContext(DbContextOptions<KoffBotContext> options)
        : base(options)
    {
    }

    public virtual DbSet<LogDrunk> LogDrunks { get; set; }

    public virtual DbSet<LogFriday> LogFridays { get; set; }

    public virtual DbSet<LogPrice> LogPrices { get; set; }

    public virtual DbSet<LogToast> LogToasts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.\\SQLExpress;Database=KoffBot;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LogDrunk>(entity =>
        {
            entity.ToTable("LogDrunk");

            entity.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedBy)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<LogFriday>(entity =>
        {
            entity.ToTable("LogFriday");

            entity.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedBy)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<LogPrice>(entity =>
        {
            entity.ToTable("LogPrice");

            entity.Property(e => e.Amount)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedBy)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<LogToast>(entity =>
        {
            entity.ToTable("LogToast");

            entity.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedBy)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
