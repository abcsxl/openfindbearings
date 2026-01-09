# OpenFindBearings 设计文档

## 文档目录

| 文档 | 说明 |
|------|------|
| [01-architecture-overview.md](./01-architecture-overview.md) | 架构概览（基于 Dapr） |
| [02-api-design.md](./02-api-design.md) | API 接口设计 |
| [03-database-design.md](./03-database-design.md) | 数据库设计 |
| [04-dapr-implementation.md](./04-dapr-implementation.md) | Dapr 实施指南 |

## 快速导航

### 架构相关
- [Dapr 架构概览](./01-architecture-overview.md#dapr-架构图)
- [Dapr 构建块使用](./01-architecture-overview.md#dapr-构建块使用)
- [技术栈对比](./01-architecture-overview.md#dapr-优势)
- [项目结构](./01-architecture-overview.md#项目结构)

### Dapr 相关
- [Dapr 配置文件](./04-dapr-implementation.md#dapr-配置文件)
- [Dapr Service Invocation](./04-dapr-implementation.md#服务调用)
- [Dapr Pub/Sub](./04-dapr-implementation.md#发布订阅)
- [Dapr State Management](./04-dapr-implementation.md#状态管理)
- [Dapr Actors](./04-dapr-implementation.md#actor-模型)

### API 相关
- [API 规范](./02-api-design.md#api-规范)
- [认证服务 API](./02-api-design.md#1-认证服务-auth-service)
- [用户服务 API](./02-api-design.md#2-用户服务-user-service)
- [轴承服务 API](./02-api-design.md#3-轴承服务-bearing-service)
- [库存服务 API](./02-api-design.md#4-库存服务-inventory-service)
- [询价交易 API](./02-api-design.md#5-询价交易服务-inquiry-service)
- [匹配服务 API](./02-api-design.md#6-匹配服务-match-service)
- [错误码定义](./02-api-design.md#错误码)

### 数据库相关
- [数据库列表](./03-database-design.md#数据库列表)
- [认证数据库 (auth_db)](./03-database-design.md#1-auth_db---用户认证数据库)
- [用户数据库 (user_db)](./03-database-design.md#2-user_db---用户信息数据库)
- [轴承数据库 (bearing_db)](./03-database-design.md#3-bearing_db---轴承主数据库)
- [库存数据库 (inventory_db)](./03-database-design.md#4-inventory_db---库存管理数据库)
- [询价交易数据库 (inquiry_db)](./03-database-design.md#5-inquiry_db---询价交易数据库)
- [匹配数据库 (match_db)](./03-database-design.md#6-match_db---匹配记录数据库)
- [消息数据库 (message_db)](./03-database-design.md#7-message_db---消息系统数据库)

## 架构概览

```
                    ┌─────────────────────────────────┐
                    │      API Gateway (YARP)        │
                    │           Port: 5000             │
                    └─────────────────────────────────┘
                                   │
        ┌──────────────────────────┼──────────────────────────┐
        │                          │                          │
        ▼                          ▼                          ▼
┌───────────────┐        ┌───────────────┐        ┌───────────────┐
│ Auth Service  │        │ Bearing Svc   │        │ Inventory Svc │
│ + Dapr Sidecar│        │ + Dapr Sidecar│        │ + Dapr Sidecar│
│ Port: 5001    │        │ Port: 5003    │        │ Port: 5004    │
│ Dapr: 3501    │        │ Dapr: 3503    │        │ Dapr: 3504    │
└───────────────┘        └───────────────┘        └───────────────┘
        │                          │                          │
        └──────────────────────────┼──────────────────────────┘
                                   │
                    ┌──────────────▼───────────────┐
                    │      Dapr Pub/Sub (Redis)    │
                    └──────────────────────────────┘
```

## 核心技术栈

| 组件 | 技术 | 版本 |
|------|------|------|
| 开发框架 | .NET | 10.0 |
| 运行时 | Dapr | 1.13+ |
| 数据库 | PostgreSQL | 15 |
| 缓存/状态存储 | Redis | 7 |
| 认证授权 | OpenIddict | 4.x |
| 分布式追踪 | Zipkin | Latest |
| API 网关 | YARP | Latest |

## Dapr 关键特性

### 服务调用
```csharp
// 自动服务发现，无需关心服务地址
await _daprClient.InvokeMethodAsync<BearingDto>(
    HttpMethod.Get,
    "bearing-service",
    "api/bearings/1");
```

### 发布订阅
```csharp
// 发布事件
await _daprClient.PublishEventAsync("pubsub", "stock-changed", @event);

// 订阅事件
[HttpPost("stock-changed")]
[Topic("pubsub", "stock-changed")]
public async Task HandleStockChanged([FromBody] StockChangedEvent @event) { }
```

### Actor 模型
```csharp
// 调用 Actor
var proxy = _actorProxyFactory.CreateActorProxy<IInventoryActor>(
    new ActorId("123"), "InventoryActor");
await proxy.ReserveStockAsync(orderId, quantity);
```

## 开发进度

- [x] 项目结构搭建（37 个项目）
- [x] 共享基础设施代码
- [x] 数据库初始化脚本
- [x] 设计文档编写
- [x] **Dapr 架构设计**
- [ ] Dapr 环境安装
- [ ] Dapr 配置文件创建
- [ ] Shared.Dapr 项目创建
- [ ] Notification Service 迁移（Dapr 试点）
- [ ] Inventory Service Actor 实现
- [ ] Match Service Actor 实现
- [ ] 其他服务迁移
- [ ] 前端开发
- [ ] 测试与部署

## 迁移计划

| 阶段 | 工作量 | 说明 |
|------|--------|------|
| Dapr 基础设施 | 1 周 | 安装、配置、Docker Compose 更新 |
| 共享层重构 | 1 周 | Shared.Dapr 项目、移除 MassTransit |
| Notification Service 迁移 | 3 天 | 试点服务 |
| Inventory Service 迁移 | 1 周 | 实现 Actor |
| Match Service 迁移 | 1 周 | 实现 Actor |
| 其他服务迁移 | 1 周 | Bearing、User、Inquiry、Message |
| Auth Service 迁移 | 3 天 | 特殊处理 |
| 测试优化 | 1 周 | 端到端测试、性能优化 |
| **总计** | **6-7 周** | |

## 文档更新记录

| 日期 | 版本 | 说明 |
|------|------|------|
| 2025-01-09 | 2.0 | 迁移到 Dapr 架构 |
| 2025-01-09 | 1.0 | 初始版本（传统微服务架构） |
