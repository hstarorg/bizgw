using GatewayServer.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GatewayServer.Data
{
    public class GatewayDbContext : DbContext
    {
        public GatewayDbContext(DbContextOptions<GatewayDbContext> options) : base(options)
        {
        }

        public DbSet<RouteEntity> Routes => Set<RouteEntity>();
        public DbSet<ClusterEntity> Clusters => Set<ClusterEntity>();
        public DbSet<ClusterDestinationEntity> Destinations => Set<ClusterDestinationEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 逻辑删除：默认只查未删除的数据
            modelBuilder.Entity<RouteEntity>().HasQueryFilter(x => x.IsDeleted == 0);
            modelBuilder.Entity<ClusterEntity>().HasQueryFilter(x => x.IsDeleted == 0);
            modelBuilder.Entity<ClusterDestinationEntity>().HasQueryFilter(x => x.IsDeleted == 0);
        }
    }
}
