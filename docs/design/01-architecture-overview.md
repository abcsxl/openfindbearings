# OpenFindBearings 轴承交易平台 - 架构设计文档

## 项目概述

基于 **.NET 10 + Dapr** 的云原生微服务架构轴承交易平台，支持供应商管理库存、发布寻货询价、智能匹配供需信息。

## 微服务架构设计

### 服务划分

| 服务 | 端口 | Dapr 端口 | 职责 |
|------|------|----------|------|
| API Gateway (YARP) | 5000 | - | 统一入口、路由、认证 |
| Auth Service | 5001 | 3501 | OpenIddict 认证授权 |
| User Service | 5002 | 3502 | 用户/企业管理、信用评级 |
| Bearing Service | 5003 | 3503 | 轴承主数据管理 |
| Inventory Service | 5004 | 3504 | 库存管理（使用 Actor） |
| Inquiry Service | 5005 | 3505 | 询价、报价、订单 |
| Match Service | 5006 | 3506 | 智能供需匹配（使用 Actor） |
| Message Service | 5007 | 3507 | 站内消息、WebSocket |
| Notification Service | 5008 | 3508 | 短信/邮件/推送通知 |
| Background Service | 5009 | 3509 | 定时任务、数据同步 |

### 核心技术栈

| 组件 | 技术 | 版本 | 说明 |
|------|------|------|------|
| 开发框架 | .NET | 10.0 | 主开发框架 |
| 运行时 | Dapr | 1.13+ | 分布式应用运行时 |
| 数据库 | PostgreSQL | 15 | 持久化存储 |
| 缓存/状态存储 | Redis | 7 | 缓存 + Dapr 状态存储 + Pub/Sub |
| 认证授权 | OpenIddict | 4.x | OAuth2/OIDC |
| 分布式追踪 | Zipkin | Latest | Dapr 追踪集成 |
| API 网关 | YARP | Latest | 反向代理 |
| 容器化 | Docker Compose | Latest | 本地开发部署 |

## Dapr 架构图

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         Web Frontend (Blazor/React)                        │
└─────────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                        API Gateway (YARP + Dapr)                           │
│                     - 路由转发、统一认证、负载均衡                          │
└─────────────────────────────────────────────────────────────────────────────┘
                                    │
        ┌───────────────────────────┼───────────────────────────┐
        │                           │                           │
        ▼                           ▼                           ▼
┌───────────────┐          ┌───────────────┐          ┌───────────────┐
│ Auth Service  │          │ Bearing Svc   │          │ Inventory Svc │
│ + Dapr Sidecar│          │ + Dapr Sidecar│          │ + Dapr Sidecar│
│ Port: 5001    │          │ Port: 5003    │          │ Port: 5004    │
│ Dapr: 3501    │          │ Dapr: 3503    │          │ Dapr: 3504    │
└───────────────┘          └───────────────┘          └───────────────┘
        │                           │                           │
        └───────────────────────────┼───────────────────────────┘
                                    │
                    ┌───────────────┴───────────────┐
                    │       Dapr Pub/Sub (Redis)    │
                    │   - StockChangedEvent        │
                    │   - InquiryCreatedEvent      │
                    │   - QuotationCreatedEvent    │
                    │   - MatchCompletedEvent      │
                    └───────────────────────────────┘
                                    │
                    ┌───────────────┴───────────────┐
                    │       Dapr Placement          │
                    │       (Actor 分布)            │
                    └───────────────────────────────┘
```

### Dapr 构建块使用

| 构建块 | 使用场景 | 说明 |
|--------|----------|------|
| **Service Invocation** | 服务间 HTTP 调用 | 自动服务发现、重试、mTLS |
| **Publish & Subscribe** | 异步事件通知 | 替代 MassTransit + RabbitMQ |
| **State Management** | 库存锁、用户会话、分布式锁 | 短期状态存储 |
| **Actors** | 库存管理、询价匹配 | 虚拟 Actor 模型 |
| **Observability** | 分布式追踪 | Zipkin 集成 |

## 项目结构

```
openfindbearings/
├── docs/                          # 文档目录
│   ├── design/                    # 设计文档
│   ├── api/                       # API 文档
│   └── database/                  # 数据库文档
├── dapr/                          # Dapr 配置目录
│   ├── config.yaml                # Dapr 全局配置
│   └── components/                # Dapr 组件配置
│       ├── pubsub.yaml           # Pub/Sub 组件
│       ├── statestore.yaml       # 状态存储组件
│       ├── actorstore.yaml       # Actor 状态存储
│       └── secretstore.yaml      # Secret 存储
├── infrastructure/                # 基础设施
│   ├── scripts/                   # 脚本文件
│   │   └── init-db.sql           # 数据库初始化
│   └── monitoring/                # 监控配置
├── src/                           # 源代码
│   ├── services/                  # 微服务
│   │   ├── Auth.Service/          # 认证服务
│   │   │   ├── Auth.Api/          # API 层
│   │   │   ├── Auth.Core/         # 核心业务层
│   │   │   ├── Auth.Infrastructure/# 基础设施层
│   │   │   └── Auth.Domain/       # 领域层
│   │   ├── User.Service/          # 用户服务
│   │   ├── Bearing.Service/       # 轴承主数据服务
│   │   ├── Inventory.Service/     # 库存管理服务（Actor）
│   │   │   └── Inventory.Api/
│   │   │       └── Actors/        # Actor 实现
│   │   ├── Inquiry.Service/       # 询价交易服务
│   │   ├── Match.Service/         # 智能匹配服务（Actor）
│   │   │   └── Match.Api/
│   │   │       └── Actors/        # Actor 实现
│   │   ├── Message.Service/       # 消息服务
│   │   ├── Notification.Service/  # 通知服务
│   │   └── Background.Service/    # 后台处理服务
│   ├── gateway/                   # 网关
│   │   └── ApiGateway/            # API 网关
│   ├── web/                       # 前端
│   └── shared/                    # 共享库
│       ├── Shared.Domain/         # 共享领域层
│       ├── Shared.Dapr/           # Dapr 共享组件（新增）
│       │   ├── Services/          # Dapr 服务封装
│       │   └── Actors/            # Actor 接口定义
│       ├── Shared.Application/    # 共享应用层
│       ├── Shared.Infrastructure/ # 共享基础设施层
│       └── Shared.DTOs/           # 共享 DTO
├── tests/                         # 测试
│   ├── Unit.Tests/                # 单元测试
│   └── Integration.Tests/         # 集成测试
├── docker-compose.yml             # Docker Compose 配置
├── .env.example                   # 环境变量示例
└── README.md                      # 项目说明
```

## 数据库设计

### 数据库列表

| 数据库 | 用途 | 主要表 |
|--------|------|--------|
| auth_db | 用户认证 | users, roles, user_roles, refresh_tokens |
| user_db | 用户信息 | companies, company_documents, credit_records |
| bearing_db | 轴承主数据 | brands, categories, bearing_models, bearing_aliases |
| inventory_db | 库存管理 | inventory_items, inventory_records, price_history |
| inquiry_db | 询价交易 | inquiries, quotations, orders, order_status_history |
| match_db | 匹配记录 | match_records, recommendations |
| message_db | 消息系统 | message_threads, messages |

## 消息事件设计

### Dapr Pub/Sub 主题

| 主题 | 事件类型 | 生产者 | 消费者 |
|------|----------|--------|--------|
| stock-changed | StockChangedEvent | Inventory Service | Match Service |
| inquiry-created | InquiryCreatedEvent | Inquiry Service | Match Service, Notification Service |
| quotation-created | QuotationCreatedEvent | Inquiry Service | Notification Service |
| match-completed | MatchCompletedEvent | Match Service | Notification Service |
| order-status-changed | OrderStatusChangedEvent | Inquiry Service | Notification Service |
| user-registered | UserRegisteredEvent | Auth Service | Notification Service |

### 订阅示例

```csharp
[HttpPost("stock-changed")]
[Topic("pubsub", "stock-changed")]
public async Task HandleStockChanged([FromBody] StockChangedEvent @event)
{
    // 处理库存变更事件
}
```

## Dapr 通信流程

### 服务间调用

```
┌─────────────────┐      ┌─────────────────┐      ┌─────────────────┐
│ Inquiry Service │      │  Dapr Sidecar  │      │  Dapr Sidecar  │
│   :5005         │─────>│    :3505        │─────>│    :3503        │
└─────────────────┘      └─────────────────┘      └─────────────────┘
                                                        │
                                                        ▼
                                               ┌─────────────────┐
                                               │ Bearing Service  │
                                               │    :5003         │
                                               └─────────────────┘

代码示例：
await _daprClient.InvokeMethodAsync<BearingDto>(
    HttpMethod.Get,
    "bearing-service",
    "api/bearings/1");
```

### 发布订阅

```
┌─────────────────┐      ┌─────────────────┐      ┌─────────────────┐
│ Inventory Svc   │      │  Dapr Pub/Sub  │      │  Dapr Pub/Sub  │
│   :5004         │─────>│   (Redis)       │─────>│   (Redis)       │
└─────────────────┘      └─────────────────┘      └─────────────────┘
                                                        │
                                        ┌───────────────┬──┴────────┐
                                        ▼               ▼             ▼
                                ┌───────────┐  ┌───────────┐  ┌───────────┐
                                │Match Svc  │  │Notifi Svc │  │Background │
                                │  :5006    │  │  :5008    │  │  :5009    │
                                └───────────┘  └───────────┘  └───────────┘

发布代码：
await _daprClient.PublishEventAsync("pubsub", "stock-changed", @event);
```

### Actor 调用

```
┌─────────────────┐      ┌─────────────────┐      ┌─────────────────┐
│ Order Service   │      │Inventory Actor  │      │Inventory Actor  │
│   :5005         │─────>│   Placement     │─────>│   Instance 1    │
└─────────────────┘      └─────────────────┘      └─────────────────┘
                                                         │
                                                         ▼
                                                  ┌─────────────────┐
                                                  │ Inventory State  │
                                                  │  (Redis Actor)   │
                                                  └─────────────────┘

代码示例：
var proxy = _actorProxyFactory.CreateActorProxy<IInventoryActor>(
    new ActorId("123"), "InventoryActor");
await proxy.ReserveStockAsync(orderId, quantity);
```

## 认证授权流程

```
┌─────────────┐          ┌──────────────┐          ┌─────────────┐
│   Client    │          │ Auth Service │          │   Resource  │
│  (Web/App)  │          │  (OpenIddict)│          │   Services  │
└──────┬──────┘          └──────┬───────┘          └──────┬──────┘
       │                        │                         │
       │  1. POST /auth/login   │                         │
       │  (username+password)   │                         │
       ├───────────────────────>│                         │
       │                        │                         │
       │  2. Validate user     │                         │
       │                        ├─────────────────────>    │
       │                        │                         │
       │  3. User info         │                         │
       │                        │<─────────────────────    │
       │                        │                         │
       │  4. JWT Token         │                         │
       │<───────────────────────│                         │
       │                        │                         │
       │  5. API + Bearer Token │                         │
       ├─────────────────────────────────────────────────>│
       │                        │                         │
       │  6. Response Data     │                         │
       │<─────────────────────────────────────────────────│
```

## API 设计规范

### 统一响应模型

```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T? Data { get; set; }
    public string? ErrorCode { get; set; }
    public DateTime Timestamp { get; set; }
}

public class PagedResponse<T> : ApiResponse<List<T>>
{
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
}
```

## 技术要点

### Clean Architecture 分层

每个微服务采用四层架构：

- **Api 层** - 控制器、路由、Dapr 订阅
- **Core 层** - 业务逻辑、服务接口
- **Infrastructure 层** - 数据访问、Dapr 集成
- **Domain 层** - 实体、值对象、领域事件

### Dapr 优势

| 方面 | 传统架构 | Dapr 架构 |
|------|----------|-----------|
| 服务发现 | 需要注册中心 | Dapr 自动处理 |
| 消息队列 | RabbitMQ 集群 | Redis Pub/Sub |
| 状态管理 | 数据库/Redis | Dapr State Store |
| 分布式追踪 | 需要额外配置 | Dapr 内置 |
| 负载均衡 | 需要配置 | Dapr Sidecar 处理 |
| mTLS | 需要配置 | Dapr 自动处理 |
| Actor 模型 | 需要自己实现 | Dapr Actors |

### 数据一致性策略

- 每个微服务独立数据库
- 通过 Dapr Pub/Sub 实现最终一致性
- 使用 Dapr State Management 处理短期状态
- 使用 Dapr Actors 处理并发状态

### Actor 使用场景

**库存管理 Actor (InventoryActor)：**
- 每个库存项一个 Actor
- 并发安全的库存操作
- 库存预留机制
- 自动过期提醒

**询价匹配 Actor (InquiryMatchActor)：**
- 每个询价一个 Actor
- 异步匹配供应商
- 匹配结果缓存

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
