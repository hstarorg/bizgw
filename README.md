# bizgw

A simple gateway based on Yarp.ReverseProxy (.NET 9)

## 如何使用

> 大家如果对不想折腾构建打包这些，也可以直接拉取我推送的镜像进行体验：https://hub.docker.com/repository/docker/hstarorg/gateway

待补充...

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

- GatewayServer 核心服务，网关入口（可部署）
  - GatewayServer.AsyncProxyConfig 获取远程代理配置项目
  - GatewayServer.Tests 单元测试项目
- GatewayServer.ConfigrationAPI 管理类 API，用于提供网关配置的 API 接口
- web-ui 主要是基于 `umi` 编写的管理后台，与 `GatewayServer.ConfigrationAPI` 配合即可进行可视化的网关配置管理

为什么没有将管理类 API 放在网关入口项目中？

- 保障级别不一样，`GatewayServer` 是高保项目，而 `GatewayServer.ConfigrationAPI` 保障级别一般
- 部署量级不一样， `GatewayServer` 需要较多的实例，而 `GatewayServer.ConfigrationAPI` 较少（甚至可以单实例）实例即可

### 部署说明

本项目基于 **.NET 9**，推荐采用 `docker` 镜像化部署（支持 Linux）。其中 GatewayServer 需要外网，另外的项目（WebAPI 和 UI）建议仅部署在内网。

> 容器内服务监听 **8080** 端口（.NET 8+ 官方镜像默认的非 root 端口），不再是旧版的 80。

#### 1、安装数据库

首先是初始化 DB（当前使用的是 Mysql），数据库文件在：docs 下，按照日期进行了分类，优先选择最新的日期。

> 注意：`docs` 下的 SQL 使用了 `GO` 分隔符且不含 `CREATE DATABASE`，导入前需先创建好数据库并去掉 `GO` 行。

#### 2、镜像构建（从源码构建，无需 Visual Studio 发布）

各项目的 `Dockerfile` 已是自包含的多阶段构建（restore → publish → 运行）。**构建上下文为解决方案根目录**：

```bash
# 构建网关镜像
docker build -f GatewayServer/Dockerfile -t bizgw/gateway:latest .

# 构建管理 API 镜像
docker build -f GatewayServer.ConfigrationAPI/Dockerfile -t bizgw/config-api:latest .
```

#### 3、启动容器
> 请注意：在我们的代码实现中，依赖两个环境变量：
> 其中 ConnectionString 是必须依赖，用于告诉 Server 连接到那个 DB 去读取代理配置，需要配置为 mysql 连接字符串
> 其中 AuthCode 是可选依赖，用于在执行 reload 的时候做身份校验。如果没有配置，会自动生成一个 AuthCode（每次重启均会变化，推荐直接用环境变量锁定），需要在日志中去查看

```bash
# 8889 映射到容器内的 8080
docker run -p 8889:8080 -d --name gateway01 -e AuthCode="1234567897854545" -e ConnectionString="server=192.168.31.250;port=3306;uid=root;pwd=localDev;database=gatewaydb" bizgw/gateway:latest
```

访问 `服务器IP:8889` 即可访问网关服务器。

#### 4、使用 docker compose 一键部署（含 MySQL，便于本地/测试）

```bash
docker compose up -d --build
```

该编排会启动 `db`(MySQL) + `gateway`(8889) + `config-api`(8890)。首次启动后需按第 1 步导入数据库表结构。