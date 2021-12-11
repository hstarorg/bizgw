# bizgw
A simple gateway based on Yarp.ReverseProxy

## 如何使用

待补充...

## 项目说明

### 结构说明
整个项目主要分为三个部分：

- GatewayServer 核心服务，网关入口（可部署）
  - GatewayServer.AsyncProxyConfig 获取远程代理配置项目
  - GatewayServer.Tests 单元测试项目
- GatewayServer.ConfigrationAPI 管理类API，用于提供网关配置的 API 接口
- web-ui 主要是基于 `umi` 编写的管理后台，与 `GatewayServer.ConfigrationAPI` 配合即可进行可视化的网关配置管理

为什么没有将管理类 API 放在网关入口项目中？
- 保障级别不一样，`GatewayServer` 是高保项目，而 `GatewayServer.ConfigrationAPI` 保障级别一般
- 部署量级不一样， `GatewayServer` 需要较多的实例，而 `GatewayServer.ConfigrationAPI` 较少（甚至可以单实例）实例即可


### 部署说明

整体推荐采用 `docker` 镜像化部署，其中 GatewayServer 需要外网，另外的项目（WebAPI 和 UI）建议仅部署在内网。
