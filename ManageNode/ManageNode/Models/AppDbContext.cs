using ManageNode.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ManageNode.Models
{
    public class AppDbContext: DbContext
    {
        public DbSet<Files> Files { get; set; }
        public DbSet<Statistics> Statistics { get;set; }
        public DbSet<MapFile> MapFiles { get; set; }
        public DbSet<SortData> SortDatas { get; set; }
        public DbSet<Shuffle> Shuffles { get; set; }
        public DbSet<ReduceFile> ReduceFiles { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=MapReduceDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
        }
        public AppDbContext(DbContextOptions<AppDbContext> options): base(options) { }
    }
}
