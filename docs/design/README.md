# OpenFindBearings 设计文档

## 文档目录

| 文档 | 说明 |
|------|------|
| [01-architecture-overview.md](./01-architecture-overview.md) | 架构概览、技术栈、服务划分 |
| [02-api-design.md](./02-api-design.md) | API 接口设计、请求响应格式 |
| [03-database-design.md](./03-database-design.md) | 数据库设计、表结构、索引 |

## 快速导航

### 架构相关
- [微服务架构设计](./01-architecture-overview.md#微服务架构设计)
- [技术栈选型](./01-architecture-overview.md#核心技术栈)
- [项目结构](./01-architecture-overview.md#项目结构)

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
└───────────────┘        └───────────────┘        └───────────────┘
        │                          │                          │
        └──────────────────────────┼──────────────────────────┘
                                   │
                    ┌──────────────▼───────────────┐
                    │      RabbitMQ 消息总线        │
                    └──────────────────────────────┘
```

## 技术栈

| 组件 | 技术 | 版本 |
|------|------|------|
| 开发框架 | .NET | 10.0 |
| 数据库 | PostgreSQL | 15 |
| 缓存 | Redis | 7 |
| 消息队列 | RabbitMQ | 3 |
| 认证授权 | OpenIddict | 4.x |
| API 网关 | Ocelot/YARP | Latest |

## 开发进度

- [x] 项目结构搭建 (37 个项目)
- [x] 共享基础设施代码
- [x] 数据库初始化脚本
- [x] 设计文档编写
- [ ] Auth Service 实现
- [ ] User Service 实现
- [ ] Bearing Service 实现
- [ ] Inventory Service 实现
- [ ] Inquiry Service 实现
- [ ] Match Service 实现
- [ ] API Gateway 配置
- [ ] 前端开发
- [ ] 测试与部署

## 文档更新记录

| 日期 | 版本 | 说明 |
|------|------|------|
| 2025-01-09 | 1.0 | 初始版本 |
