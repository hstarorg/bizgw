using System.Text.Json;
using GatewayServer.ControlPlane.Auth;
using GatewayServer.ControlPlane.Dtos;
using GatewayServer.ControlPlane.Http;
using GatewayServer.Data;
using GatewayServer.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GatewayServer.ControlPlane.Services
{
    /// <summary>路由编辑(写编辑表,不自动发布)。</summary>
    public sealed class RouteService(IDbContextFactory<GatewayDbContext> dbf, ICurrentUser current)
    {
        public async Task<(int total, List<RouteDto> items)> ListAsync(int page, int size, string? keyword)
        {
            page = page < 1 ? 1 : page;
            size = size < 1 ? 20 : Math.Min(size, 200);

            await using var db = await dbf.CreateDbContextAsync();
            var q = db.Routes.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                q = q.Where(x => x.RouteName.Contains(keyword)
                    || x.ClusterCode.Contains(keyword)
                    || x.MatchPath.Contains(keyword));
            }

            var total = await q.CountAsync();
            var rows = await q.OrderByDescending(x => x.Id).Skip((page - 1) * size).Take(size).ToListAsync();
            return (total, rows.Select(ToDto).ToList());
        }

        public async Task<RouteDto> GetAsync(long id)
        {
            await using var db = await dbf.CreateDbContextAsync();
            var row = await db.Routes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw ApiException.NotFound("路由不存在");
            return ToDto(row);
        }

        public async Task<RouteDto> CreateAsync(RouteUpsertRequest req)
        {
            var transforms = NormalizeTransforms(req.Transforms);
            await using var db = await dbf.CreateDbContextAsync();
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var row = new RouteEntity
            {
                RouteName = req.RouteName,
                ClusterCode = req.ClusterCode,
                MatchPath = req.MatchPath,
                MatchMethods = string.Join('|', req.MatchMethods),
                Transforms = transforms,
                Remark = req.Remark,
                CreatorName = By,
                ModifierName = By,
                CreateDate = now,
                ModifyDate = now,
            };
            db.Routes.Add(row);
            await db.SaveChangesAsync();
            return ToDto(row);
        }

        public async Task UpdateAsync(long id, RouteUpsertRequest req)
        {
            var transforms = NormalizeTransforms(req.Transforms);
            await using var db = await dbf.CreateDbContextAsync();
            var row = await db.Routes.FirstOrDefaultAsync(x => x.Id == id)
                ?? throw ApiException.NotFound("路由不存在");
            row.RouteName = req.RouteName;
            row.ClusterCode = req.ClusterCode;
            row.MatchPath = req.MatchPath;
            row.MatchMethods = string.Join('|', req.MatchMethods);
            row.Transforms = transforms;
            row.Remark = req.Remark;
            row.ModifierName = By;
            row.ModifyDate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(long id)
        {
            await using var db = await dbf.CreateDbContextAsync();
            var row = await db.Routes.FirstOrDefaultAsync(x => x.Id == id)
                ?? throw ApiException.NotFound("路由不存在");
            row.IsDeleted = 1;
            row.ModifierName = By;
            row.ModifyDate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            await db.SaveChangesAsync();
        }

        private string By => current.Name ?? "system";

        /// <summary>transforms 必须是合法 JSON 数组;空则归一为 "[]"。</summary>
        private static string NormalizeTransforms(string? transforms)
        {
            if (string.IsNullOrWhiteSpace(transforms)) return "[]";
            try
            {
                using var doc = JsonDocument.Parse(transforms);
                if (doc.RootElement.ValueKind != JsonValueKind.Array)
                    throw ApiException.BadRequest("transforms 必须是 JSON 数组");
            }
            catch (JsonException)
            {
                throw ApiException.BadRequest("transforms 不是合法 JSON");
            }
            return transforms;
        }

        private static RouteDto ToDto(RouteEntity r) => new()
        {
            Id = r.Id,
            RouteName = r.RouteName,
            ClusterCode = r.ClusterCode,
            MatchPath = r.MatchPath,
            MatchMethods = string.IsNullOrEmpty(r.MatchMethods) ? [] : r.MatchMethods.Split('|'),
            Transforms = r.Transforms,
            Remark = r.Remark,
            CreateDate = r.CreateDate,
            ModifyDate = r.ModifyDate,
            CreatorName = r.CreatorName,
            ModifierName = r.ModifierName,
        };
    }
}
