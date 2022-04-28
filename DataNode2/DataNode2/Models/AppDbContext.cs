
using Microsoft.EntityFrameworkCore;

namespace DataNode2.Models
{
    public class AppDbContext : DbContext
    {
        public DbSet<SubPart> SubFiles { get; set; }
        public DbSet<Map> Maps { get; set; }
        public DbSet<SortData> SortDatas { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<Shuffle> Shuffle { get; set; }
        public DbSet<Reduce> Reduce { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=DataNode2DB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
        }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    }
}
