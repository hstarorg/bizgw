using GatewayServer.ControlPlane.Auth;
using GatewayServer.ControlPlane.Dtos;
using GatewayServer.ControlPlane.Http;
using GatewayServer.Data;
using GatewayServer.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GatewayServer.ControlPlane.Services
{
    /// <summary>集群目标(destination)编辑,按 cluster_code 归属。</summary>
    public sealed class DestinationService(IDbContextFactory<GatewayDbContext> dbf, ICurrentUser current)
    {
        public async Task<List<DestinationDto>> ListAsync(string clusterCode)
        {
            await using var db = await dbf.CreateDbContextAsync();
            var rows = await db.Destinations.AsNoTracking()
                .Where(x => x.ClusterCode == clusterCode).OrderBy(x => x.Id).ToListAsync();
            return rows.Select(ToDto).ToList();
        }

        public async Task<DestinationDto> CreateAsync(string clusterCode, DestinationUpsertRequest req)
        {
            await using var db = await dbf.CreateDbContextAsync();
            if (!await db.Clusters.AnyAsync(x => x.ClusterCode == clusterCode))
                throw ApiException.NotFound($"集群不存在:{clusterCode}");

            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var row = new ClusterDestinationEntity
            {
                ClusterCode = clusterCode,
                Address = req.Address,
                HealthCheckPath = req.HealthCheckPath,
                Name = req.Name,
                CreatorName = By,
                ModifierName = By,
                CreateDate = now,
                ModifyDate = now,
            };
            db.Destinations.Add(row);
            await db.SaveChangesAsync();
            return ToDto(row);
        }

        public async Task UpdateAsync(string clusterCode, long id, DestinationUpsertRequest req)
        {
            await using var db = await dbf.CreateDbContextAsync();
            var row = await db.Destinations.FirstOrDefaultAsync(x => x.Id == id && x.ClusterCode == clusterCode)
                ?? throw ApiException.NotFound("目标不存在");
            row.Address = req.Address;
            row.HealthCheckPath = req.HealthCheckPath;
            row.Name = req.Name;
            row.ModifierName = By;
            row.ModifyDate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(string clusterCode, long id)
        {
            await using var db = await dbf.CreateDbContextAsync();
            var row = await db.Destinations.FirstOrDefaultAsync(x => x.Id == id && x.ClusterCode == clusterCode)
                ?? throw ApiException.NotFound("目标不存在");
            row.IsDeleted = 1;
            row.ModifierName = By;
            row.ModifyDate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            await db.SaveChangesAsync();
        }

        private string By => current.Name ?? "system";

        private static DestinationDto ToDto(ClusterDestinationEntity d) => new()
        {
            Id = d.Id,
            ClusterCode = d.ClusterCode,
            Address = d.Address,
            HealthCheckPath = d.HealthCheckPath,
            Name = d.Name,
            CreateDate = d.CreateDate,
            ModifyDate = d.ModifyDate,
            CreatorName = d.CreatorName,
            ModifierName = d.ModifierName,
        };
    }
}
