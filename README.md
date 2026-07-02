# bizgw

A simple gateway based on Yarp.ReverseProxy (.NET 10)

## 如何使用

> 大家如果对不想折腾构建打包这些，也可以直接拉取我推送的镜像进行体验：https://hub.docker.com/repository/docker/hstarorg/gateway

### 本地开发启动

前置：.NET 10 SDK、Node 22+ 与 pnpm、一个可用的 PostgreSQL。

数据库连接串统一经环境变量 `ConnectionString` 传入（Npgsql 格式，**不要写进 launchSettings/appsettings 提交到仓库**）：

```bash
# 按你的库改;Search Path 可选,用于把所有表放进指定 schema
export ConnectionString="Host=localhost;Port=5432;Username=<user>;Password=<pwd>;Database=<db>;Search Path=bizgw"
```

```bash
# 0) 初始化数据库(首次,以及每次迁移变更后)
psql -h localhost -U <user> -d <db> -c 'CREATE SCHEMA IF NOT EXISTS bizgw;'  # 用了 Search Path 才需要
dotnet tool install --global dotnet-ef                                       # 首次
dotnet ef database update --project GatewayServer.Data

# 1) 控制面(管理 API,web-ui 的后端)
dotnet run --project GatewayServer.ControlPlane --urls http://localhost:5160

# 2) 前端(另开终端;dev server 将 /api 代理到 5160)
cd web-ui && pnpm install && pnpm dev     # 打开 http://localhost:5173

# 3) (可选)网关数据面,验证发布的配置真实生效
Listeners="PostgresNotify,Polling" dotnet run --project GatewayServer --urls http://localhost:8088
```

首次打开 http://localhost:5173 会进入「初始化管理员」页——创建首位 Owner 账户后即可登录管理。

### 重新加载配置

GatewayServer 自身提供了 `/reload` API 来重新加载 proxy 配置，调用方式如下：

```js
// 其中 server:port 要换成你部署出来的地址
fetch("http://server:port/reload", {
  method: "POST",
  headers: {
    "content-type": "application/json",
  },
  body: JSON.stringify({ AuthCode: "xxxx" }), // 这个 AuthCode 就是环境变量中配置的那个，或者是自动生成的（可以在启动日志中找到）
});
```



## 项目说明

### 结构说明

整个项目主要分为三个部分：

数据面 Data Plane（跑流量，高保、多实例）

- GatewayServer 核心服务，网关入口（可部署）；内部含异步代理配置 provider（`ConfigHelper/`、`ProxyAsyncProvider/`、`Entities/`，原 `GatewayServer.AsyncProxyConfig` 项目已并入）
  - GatewayServer.Tests 单元测试项目

控制面 Control Plane（管配置，保障级别一般、可单实例）

- GatewayServer.ControlPlane 管理类 API，用于提供网关配置的 API 接口
- web-ui 主要是基于 `umi` 编写的管理后台，与 `GatewayServer.ControlPlane` 配合即可进行可视化的网关配置管理

共享数据层

- GatewayServer.Data 实体 + `GatewayDbContext` + EF Core Migrations，控制面与数据面通过它共享 schema

为什么没有将管理类 API 放在网关入口项目中？

- 保障级别不一样，`GatewayServer` 是高保项目，而 `GatewayServer.ControlPlane` 保障级别一般
- 部署量级不一样， `GatewayServer` 需要较多的实例，而 `GatewayServer.ControlPlane` 较少（甚至可以单实例）实例即可

### 部署说明

本项目基于 **.NET 10**，推荐采用 `docker` 镜像化部署（支持 Linux）。其中 GatewayServer 需要外网，另外的项目（WebAPI 和 UI）建议仅部署在内网。

> 容器内服务监听 **8080** 端口（.NET 8+ 官方镜像默认的非 root 端口），不再是旧版的 80。

#### 1、初始化数据库

当前使用 **PostgreSQL**，数据访问基于 **EF Core**，表结构由 EF Core Migrations 管理（不再使用 `docs/*.sql` 手写脚本）。先准备好一个空库，再用迁移建表：

```bash
# 全局安装一次 ef 工具
dotnet tool install --global dotnet-ef

# 用 PG 连接串建表（ConnectionString 为 Npgsql 格式）
ConnectionString="Host=localhost;Port=5432;Username=postgres;Password=localDev;Database=gatewaydb" \
  dotnet ef database update --project GatewayServer.Data
```

> 注意：`docs/*.sql` 是旧版 MySQL 手写脚本，已弃用，仅作历史参考。

#### 2、镜像构建（从源码构建，无需 Visual Studio 发布）

各项目的 `Dockerfile` 已是自包含的多阶段构建（restore → publish → 运行）。**构建上下文为解决方案根目录**：

```bash
# 构建网关镜像
docker build -f GatewayServer/Dockerfile -t bizgw/gateway:latest .

# 构建管理 API 镜像
docker build -f GatewayServer.ControlPlane/Dockerfile -t bizgw/control-plane:latest .
```

#### 3、启动容器
> 请注意：在我们的代码实现中，依赖两个环境变量：
> 其中 ConnectionString 是必须依赖，用于告诉 Server 连接到那个 DB 去读取代理配置，需要配置为 PostgreSQL（Npgsql）连接字符串
> 其中 AuthCode 是可选依赖，用于在执行 reload 的时候做身份校验。如果没有配置，会自动生成一个 AuthCode（每次重启均会变化，推荐直接用环境变量锁定），需要在日志中去查看

```bash
# 8889 映射到容器内的 8080
docker run -p 8889:8080 -d --name gateway01 -e AuthCode="1234567897854545" -e ConnectionString="Host=192.168.31.250;Port=5432;Username=postgres;Password=localDev;Database=gatewaydb" bizgw/gateway:latest
```

访问 `服务器IP:8889` 即可访问网关服务器。

#### 4、使用 docker compose 一键部署（含 PostgreSQL，便于本地/测试）

```bash
docker compose up -d --build
```

该编排会启动 `db`(PostgreSQL 18) + `gateway`(8889) + `control-plane`(8890)。首次启动后需按第 1 步用 `dotnet ef database update` 建表（迁移不会随容器自动执行）。