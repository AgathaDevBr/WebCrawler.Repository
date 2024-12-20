using Microsoft.EntityFrameworkCore;
using System;
using WebCrawler.Entities;

namespace WebCrawler.Context
{
    public class ApplicationBdContext : DbContext
    {
        public DbSet<ExecutionInfo> InformacoesExecucao { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Configure a string de conexão aqui.
            string stringConexao = "Server=.;Database=BdBancoCrawler;Trusted_Connection=True;";
            optionsBuilder.UseSqlServer(stringConexao);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configurações adicionais do modelo, se necessário.
            modelBuilder.Entity<ExecutionInfo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.StartTime).IsRequired();
                entity.Property(e => e.EndTime).IsRequired();
                entity.Property(e => e.TotalPages).IsRequired();
                entity.Property(e => e.TotalProxies).IsRequired();
                entity.Property(e => e.JsonFilePath).IsRequired().HasMaxLength(500);
            });
        }

        public async Task SalvarInformacaoExecucaoAsync(ExecutionInfo informacaoExecucao)
        {
            await InformacoesExecucao.AddAsync(informacaoExecucao);
            await SaveChangesAsync();
        }
    }
}