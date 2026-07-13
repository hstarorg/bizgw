#!/usr/bin/env bash
# 生成可审计的幂等迁移脚本(产线用)。
# 脚本内置版本判断(查 __EFMigrationsHistory),已应用的迁移自动跳过 —— 同一脚本在任意版本的库上重复执行都安全。
# 用法: scripts/gen-migrate-sql.sh [输出路径]   默认 artifacts/migrate-<git短sha>.sql
set -euo pipefail
cd "$(dirname "$0")/.."

out="${1:-artifacts/migrate-$(git rev-parse --short HEAD).sql}"
mkdir -p "$(dirname "$out")"

# ef 设计期不连库,连接串只需非空占位
ConnectionString="Host=placeholder;Username=placeholder;Password=placeholder;Database=placeholder" \
  dotnet ef migrations script --idempotent --project GatewayServer.Data -o "$out"

echo ""
echo "已生成: $out"
echo "产线执行(先审阅、先备份):"
echo "  psql -h <host> -U <user> -d <db> -v ON_ERROR_STOP=1 -f $out"
