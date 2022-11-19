using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace FinancialDucks.Infrastructure.Models
{
    public partial class FinancialDucksContext : DbContext
    {
        public virtual DbSet<BudgetLine> BudgetLines { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<CategoryRule> CategoryRules { get; set; }
        public virtual DbSet<ScraperCommand> ScraperCommands { get; set; }
        public virtual DbSet<SourceSnapshot> SourceSnapshots { get; set; }
        public virtual DbSet<Transaction> Transactions { get; set; }
        public virtual DbSet<TransactionCategory> TransactionCategories { get; set; }
        public virtual DbSet<TransactionSource> TransactionSources { get; set; }
        public virtual DbSet<TransactionSourceType> TransactionSourceTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BudgetLine>(entity =>
            {
                entity.Property(e => e.Budget).HasColumnType("money");

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.BudgetLines)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BudgetLines_Categories");
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasIndex(e => e.Name, "NonClusteredIndex-20220402-121716")
                    .IsUnique();

                entity.Property(e => e.HierarchyId).IsRequired();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CategoryRule>(entity =>
            {
                entity.Property(e => e.AmountMax).HasColumnType("money");

                entity.Property(e => e.AmountMin).HasColumnType("money");

                entity.Property(e => e.DateMax).HasColumnType("datetime");

                entity.Property(e => e.DateMin).HasColumnType("datetime");

                entity.Property(e => e.SubstringMatch)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.CategoryRules)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CategoryRules_Categories");
            });

            modelBuilder.Entity<ScraperCommand>(entity =>
            {
                entity.HasIndex(e => new { e.SourceId, e.Sequence }, "IX_ScraperCommands")
                    .IsUnique();

                entity.Property(e => e.Selector)
                    .IsRequired()
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.Text)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.TimeoutSeconds).HasDefaultValueSql("((30))");

                entity.HasOne(d => d.Source)
                    .WithMany(p => p.ScraperCommands)
                    .HasForeignKey(d => d.SourceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ScraperCommands_TransactionSources");
            });

            modelBuilder.Entity<SourceSnapshot>(entity =>
            {
                entity.Property(e => e.Amount).HasColumnType("money");

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.HasOne(d => d.Source)
                    .WithMany(p => p.SourceSnapshots)
                    .HasForeignKey(d => d.SourceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SourceSnapshots_TransactionSources");
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.Property(e => e.Amount).HasColumnType("money");

                entity.Property(e => e.Date).HasColumnType("date");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.HasOne(d => d.Source)
                    .WithMany(p => p.Transactions)
                    .HasForeignKey(d => d.SourceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Transactions_TransactionSources");
            });

            modelBuilder.Entity<TransactionCategory>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("TransactionCategories");

                entity.Property(e => e.Amount).HasColumnType("money");

                entity.Property(e => e.Category)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Date).HasColumnType("date");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TransactionSource>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Url)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.HasOne(d => d.Type)
                    .WithMany(p => p.TransactionSources)
                    .HasForeignKey(d => d.TypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TransactionSources_TransactionSourceTypes");
            });

            modelBuilder.Entity<TransactionSourceType>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
