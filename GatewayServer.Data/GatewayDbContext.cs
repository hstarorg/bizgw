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
        public DbSet<ConfigSnapshotEntity> ConfigSnapshots => Set<ConfigSnapshotEntity>();
        public DbSet<InstanceStatusEntity> InstanceStatuses => Set<InstanceStatusEntity>();
        public DbSet<UserEntity> Users => Set<UserEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 逻辑删除：默认只查未删除的数据
            modelBuilder.Entity<RouteEntity>().HasQueryFilter(x => x.IsDeleted == 0);
            modelBuilder.Entity<ClusterEntity>().HasQueryFilter(x => x.IsDeleted == 0);
            modelBuilder.Entity<ClusterDestinationEntity>().HasQueryFilter(x => x.IsDeleted == 0);

            // 快照表：部分唯一索引保证最多只有一个 active 行
            modelBuilder.Entity<ConfigSnapshotEntity>()
                .HasIndex(x => x.IsActive)
                .IsUnique()
                .HasFilter("is_active");

            // 用户表：逻辑删除 + username 在未删除范围内唯一
            modelBuilder.Entity<UserEntity>().HasQueryFilter(x => x.IsDeleted == 0);
            modelBuilder.Entity<UserEntity>()
                .HasIndex(x => x.Username)
                .IsUnique()
                .HasFilter("is_deleted = 0");
        }
    }
}
