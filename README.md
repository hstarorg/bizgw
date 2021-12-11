# bizgw

A simple gateway based on Yarp.ReverseProxy

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

整体推荐采用 `docker` 镜像化部署，其中 GatewayServer 需要外网，另外的项目（WebAPI 和 UI）建议仅部署在内网。

#### 1、安装数据库

首先是初始化 DB（当前使用的是 Mysql），数据库文件在：docs 下，按照日期进行了分类，优先选择最新的日期。

#### 2、构建 GatewayServer

> 请注意：请不要勾选单个文件，勾选后的产物无法直接在容器内启动

然后利用 `Visual Studio` 发布：

![VS 发布](https://cdn1.hstar.vip/20211211221306.png)

构建好后，可以直接打开文件目录

#### 3、镜像构建

先编写镜像构建文件 `Dockerfile`：

```
#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM base AS final
WORKDIR /app
COPY ["./", "/app"]
ENTRYPOINT ["dotnet", "GatewayServer.dll"]
```

接着把第二步中的产物和 `Dokerfile` 放在一个目录中，如下：

![构建后文件](https://cdn1.hstar.vip/20211211221435.png)

将整个目录的所有文件拷贝到安装了 `Docker` 的 Linux 服务器上，执行镜像构建：

```bash
# 如下命令的意思是，基于 Dockerfile 文件，以当前目录构建出镜像 jay/gateway，版本 0.0.1
docker build -t jay/gateway:0.0.1 .
```

#### 4、启动容器
> 请注意：在我们的代码实现中，依赖两个环境变量：
> 其中 ConnectionString 是必须依赖，用于告诉 Server 连接到那个 DB 去读取代理配置，需要配置为 mysql 连接字符串
> 其中 AuthCode 是可选依赖，用于在执行 reload 的时候做身份校验。如果没有配置，会自动生成一个 AuthCode（每次重启均会变化，推荐直接用环境变量锁定），需要在日志中去查看

使用如下命令即可启动容器（注意修改你的环境变量）：

```bash
# 8889 映射到容器内的 80
docker run -p 8889:80 -d --name gateway01 -e AuthCode="1234567897854545" -e ConnectionString="server=192.168.31.250;port=3306;uid=root;pwd=localDev;database=gatewaydb" jay/gateway:0.0.1
```

访问 `服务器IP:8889` 即可访问网关服务器。