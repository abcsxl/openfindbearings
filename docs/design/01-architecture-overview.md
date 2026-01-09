# OpenFindBearings 轴承交易平台 - 架构设计文档

## 项目概述

基于 .NET 10 的微服务架构轴承交易平台，支持供应商管理库存、发布寻货询价、智能匹配供需信息。

## 微服务架构设计

### 服务划分

| 服务 | 端口 | 职责 |
|------|------|------|
| API Gateway (Ocelot/YARP) | 5000 | 统一入口、路由、认证 |
| Auth Service | 5001 | OpenIddict 认证授权 |
| User Service | 5002 | 用户/企业管理、信用评级 |
| Bearing Service | 5003 | 轴承主数据管理 |
| Inventory Service | 5004 | 库存管理、价格管理 |
| Inquiry Service | 5005 | 询价、报价、订单 |
| Match Service | 5006 | 智能供需匹配 |
| Message Service | 5007 | 站内消息、WebSocket |
| Notification Service | 5008 | 短信/邮件/推送通知 |
| Background Service | 5010 | 定时任务、数据同步 |

### 核心技术栈

| 组件 | 技术 | 版本 |
|------|------|------|
| 开发框架 | .NET | 10.0 |
| 数据库 | PostgreSQL | 15 |
| 缓存 | Redis | 7 |
| 消息队列 | RabbitMQ | 3 |
| 认证授权 | OpenIddict | 4.x |
| API 网关 | Ocelot/YARP | Latest |
| 容器化 | Docker Compose | Latest |

## 架构图

```
                    ┌─────────────────────────────────┐
                    │      API Gateway (Ocelot)       │
                    │           Port: 5000             │
                    └─────────────────────────────────┘
                                   │
        ┌──────────────────────────┼──────────────────────────┐
        │                          │                          │
        ▼                          ▼                          ▼
┌───────────────┐        ┌───────────────┐        ┌───────────────┐
│ Auth Service  │        │ Bearing Svc   │        │ Inventory Svc │
│  Port: 5001   │        │  Port: 5003   │        │  Port: 5004   │
│ OpenIddict    │        │ 轴承主数据     │        │ 库存管理       │
└───────────────┘        └───────────────┘        └───────────────┘
        │                          │                          │
        └──────────────────────────┼──────────────────────────┘
                                   │
                    ┌──────────────▼───────────────┐
                    │      RabbitMQ 消息总线        │
                    └──────────────────────────────┘
```

## 项目结构

```
openfindbearings/
├── docs/                          # 文档目录
│   ├── design/                    # 设计文档
│   ├── api/                       # API 文档
│   └── database/                  # 数据库文档
├── infrastructure/                # 基础设施
│   ├── scripts/                   # 脚本文件
│   │   └── init-db.sql           # 数据库初始化
│   └── docker/                    # Docker 配置
├── src/                           # 源代码
│   ├── services/                  # 微服务
│   │   ├── Auth.Service/          # 认证服务
│   │   │   ├── Auth.Api/          # API 层
│   │   │   ├── Auth.Core/         # 核心业务层
│   │   │   ├── Auth.Infrastructure/# 基础设施层
│   │   │   └── Auth.Domain/       # 领域层
│   │   ├── User.Service/          # 用户服务
│   │   ├── Bearing.Service/       # 轴承主数据服务
│   │   ├── Inventory.Service/     # 库存管理服务
│   │   ├── Inquiry.Service/       # 询价交易服务
│   │   ├── Match.Service/         # 智能匹配服务
│   │   ├── Message.Service/       # 消息服务
│   │   ├── Notification.Service/  # 通知服务
│   │   └── Background.Service/    # 后台处理服务
│   ├── gateway/                   # 网关
│   │   └── ApiGateway/            # API 网关
│   ├── web/                       # 前端
│   └── shared/                    # 共享库
│       ├── Shared.Domain/         # 共享领域层
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

### 集成事件

| 事件 | 触发条件 | 消费者 |
|------|----------|--------|
| StockChangedEvent | 库存变更 | Match Service |
| InquiryCreatedEvent | 发布询价 | Match Service, Notification Service |
| QuotationCreatedEvent | 新报价 | Notification Service |
| MatchCompletedEvent | 匹配完成 | Notification Service |
| OrderStatusChangedEvent | 订单状态变更 | Notification Service |
| UserRegisteredEvent | 用户注册 | Notification Service |

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
       │                        │  2. Validate user       │
       │                        ├─────────────────────>   │
       │                        │                         │
       │                        │  3. User info           │
       │                        │<─────────────────────   │
       │                        │                         │
       │  4. Access Token +     │                         │
       │     Refresh Token      │                         │
       │<───────────────────────│                         │
       │                        │                         │
       │  5. API Request with   │                         │
       │     Authorization: Bearer {token}                │
       ├─────────────────────────────────────────────────>│
       │                        │                         │
       │                        │  6. Validate Token      │
       │                        │<─────────────────────   │
       │                        │                         │
       │                        │  7. Token valid?        │
       │                        ├─────────────────────>   │
       │                        │                         │
       │  8. Response Data      │                         │
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

- **Api 层** - 控制器、路由、认证
- **Core 层** - 业务逻辑、服务接口
- **Infrastructure 层** - 数据访问、外部服务
- **Domain 层** - 实体、值对象、领域事件

### 数据一致性策略

- 每个微服务独立数据库
- 通过消息队列实现最终一致性
- 共享 Domain Events 避免服务间直接调用

### 智能匹配算法

```csharp
public async Task<List<MatchResult>> FindMatchesAsync(
    long bearingModelId, int quantity, long excludeCompanyId)
{
    // 1. 查询有库存的供应商
    // 2. 计算匹配度评分 (库存量、价格、信用、距离)
    // 3. 返回排序后的匹配列表
}
```

## 开发进度

- [x] 项目结构搭建
- [x] 共享基础设施代码
- [x] 数据库初始化脚本
- [ ] Auth Service 实现
- [ ] User Service 实现
- [ ] Bearing Service 实现
- [ ] Inventory Service 实现
- [ ] Inquiry Service 实现
- [ ] Match Service 实现
- [ ] API Gateway 配置
- [ ] 前端开发
- [ ] 测试与部署
