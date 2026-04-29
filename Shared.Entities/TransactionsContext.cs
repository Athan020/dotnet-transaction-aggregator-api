using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Shared.Entities;

public partial class TransactionsContext : DbContext
{
    public TransactionsContext()
    {
    }

    public TransactionsContext(DbContextOptions<TransactionsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Categorization> Categorizations { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Source> Sources { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresExtension("pg_stat_statements")
            .HasPostgresExtension("uuid-ossp");

        modelBuilder.Entity<Categorization>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("categorization_pkey");

            entity.ToTable("categorization", "transactions");

            entity.HasIndex(e => e.TransactionId, "idx_categorization_transaction_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Transaction).WithMany(p => p.Categorizations)
                .HasForeignKey(d => d.TransactionId)
                .HasConstraintName("categorization_transaction_id_fkey");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("categories_pkey");

            entity.ToTable("categories", "transactions");

            entity.HasIndex(e => e.Name, "categories_name_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Keywords).HasColumnName("keywords");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.SubcategoryOf).HasColumnName("subcategory_of");
            entity.Property(e => e.Version).HasColumnName("version");

            entity.HasOne(d => d.SubcategoryOfNavigation).WithMany(p => p.InverseSubcategoryOfNavigation)
                .HasForeignKey(d => d.SubcategoryOf)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("categories_subcategory_of_fkey");
        });

        modelBuilder.Entity<Source>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sources_pkey");

            entity.ToTable("sources", "transactions");

            entity.HasIndex(e => e.Name, "sources_name_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.LastSynced).HasColumnName("last_synced");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("transactions_pkey");

            entity.ToTable("transactions", "transactions");

            entity.HasIndex(e => new { e.AccountId, e.TransactionDate }, "idx_transactions_account_id_date");

            entity.HasIndex(e => e.CategoryId, "idx_transactions_category_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.Amount)
                .HasPrecision(18, 2)
                .HasColumnName("amount");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .HasColumnName("currency");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ExternalId)
                .HasMaxLength(50)
                .HasColumnName("external_id");
            entity.Property(e => e.SourceId).HasColumnName("source_id");
            entity.Property(e => e.TransactionDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("transaction_date");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Category).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("transactions_category_id_fkey");

            entity.HasOne(d => d.Source).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.SourceId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("transactions_source_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
