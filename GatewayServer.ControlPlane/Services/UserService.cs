using System.Security.Claims;
using GatewayServer.ControlPlane.Auth;
using GatewayServer.ControlPlane.Http;
using GatewayServer.Data;
using GatewayServer.Data.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GatewayServer.ControlPlane.Services
{
    /// <summary>用户聚合:认证(查/验/建首位 Owner)+ 口令哈希 + 登录主体构建。用户管理 CRUD 见 7.1b。</summary>
    public sealed class UserService(
        IDbContextFactory<GatewayDbContext> dbf,
        IPasswordHasher<UserEntity> hasher)
    {
        public async Task<bool> AnyUserAsync()
        {
            await using var db = await dbf.CreateDbContextAsync();
            return await db.Users.AnyAsync();
        }

        public async Task<UserEntity?> FindByUsernameAsync(string username)
        {
            await using var db = await dbf.CreateDbContextAsync();
            return await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Username == username);
        }

        public bool VerifyPassword(UserEntity user, string password)
            => hasher.VerifyHashedPassword(user, user.PasswordHash, password) != PasswordVerificationResult.Failed;

        /// <summary>首次初始化:仅当无任何用户时,建首位 Owner。</summary>
        public async Task<UserEntity> SetupOwnerAsync(string username, string password)
        {
            if (await AnyUserAsync()) throw ApiException.Conflict("系统已初始化");
            return await CreateAsync(username, password, Roles.Owner, username);
        }

        public async Task<UserEntity> CreateAsync(string username, string password, string role, string byUser)
        {
            if (!Roles.IsValid(role)) throw ApiException.BadRequest("非法角色");

            await using var db = await dbf.CreateDbContextAsync();
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var user = new UserEntity
            {
                Username = username,
                Role = role,
                CreatorName = byUser,
                ModifierName = byUser,
                CreateDate = now,
                ModifyDate = now,
            };
            user.PasswordHash = hasher.HashPassword(user, password);

            db.Users.Add(user);
            try { await db.SaveChangesAsync(); }
            catch (DbUpdateException) { throw ApiException.Conflict("用户名已存在"); }
            return user;
        }

        /// <summary>用登录 cookie 需要的 Name + Role claim 构建主体。</summary>
        public static ClaimsPrincipal BuildPrincipal(UserEntity user)
        {
            var identity = new ClaimsIdentity(
                [new Claim(ClaimTypes.Name, user.Username), new Claim(ClaimTypes.Role, user.Role)],
                CookieAuthenticationDefaults.AuthenticationScheme);
            return new ClaimsPrincipal(identity);
        }
    }
}
