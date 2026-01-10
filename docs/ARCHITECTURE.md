# 🏗️ OpenFindBearings 系统架构

## 📁 项目目录结构
openfindbearings/  
├── .github/workflows/           # GitHub Actions CI/CD 流水线  
├── dapr/                        # Dapr 配置和组件  
├── deploy/                      # 部署配置  
├── docs/                        # 项目文档  
├── src/                         # 源代码目录  
│   ├── apigateways/             # API 网关实现  
│   ├── mobile/                  # 移动端代码
│   │   └── maui/                # .NET MAUI 跨平台应用  
│   ├── services/                # 基于 Dapr 的微服务  
│   │   ├── identity/            # 认证和用户管理服务 (.NET 10)  
│   │   ├── supplier/            # 供应商管理服务 (.NET 10)  
│   │   ├── demand/              # 需求信息服务 (.NET 10)  
│   │   ├── quotation/           # 报价管理服务 (.NET 10)  
│   │   ├── bearing/             # 轴承管理服务 (.NET 10)  
│   │   ├── inventory/           # 库存管理服务 (.NET 10)  
│   │   ├── notification/        # 通知服务 (.NET 10)  
│   │   └── order/               # 订单管理服务 (.NET 10)  
│   └── web/                     # Web 前端  
│       └── blazor/              # Blazor WebAssembly (.NET 10)  
└── README.md                    # 项目说明  

## 🎯 架构设计原则

### 1. 基于 Dapr 的微服务架构 (.NET 10)
- **服务网格**: 使用 Dapr 作为服务网格，简化服务间通信
- **多语言支持**: 不同服务可使用不同技术栈
- **可观测性**: 内置分布式追踪、指标收集
- **弹性设计**: 自动重试、断路器、限流
- **实时通信**: 支持 WebSocket、SignalR、gRPC 多种协议
### 2. 领域驱动设计 (DDD)
- 按业务能力划分服务边界
- 明确的限界上下文
- 领域模型驱动设计
- 事件驱动架构 (EDA)
### 3. 云原生设计
- 容器化部署
- 无状态服务设计
- 配置与代码分离
- 健康检查和就绪探针
- 自动扩缩容

## 🔧 详细架构说明
### 1. Dapr 配置层 (`dapr/`)
#### 1.1 组件配置
dapr/  
├── components/                 # Dapr 组件定义  
│   ├── statestore.yaml         # Redis 状态存储组件  
│   ├── pubsub.yaml             # Redis 发布订阅组件  
│   ├── bindings.yaml           # 外部系统绑定  
│   ├── secrets.yaml            # 密钥管理组件  
│   └── configuration.yaml      # Dapr 运行时配置  
└── deployments/                # Dapr 部署描述  

#### 1.2 核心组件功能
- **statestore**: 用于服务状态管理，如会话状态、缓存
- **pubsub**: 事件驱动架构，服务间解耦通信
- **bindings**: 集成外部系统（数据库、消息队列等）
- **secrets**: 安全地管理敏感信息

### 2. 部署配置层 (`deploy/`)
#### 2.1 目录结构
deploy/                          # 基础设施即代码  
├── docker/                      # Docker 配置  
│   ├── compose/  
│   │   ├── docker-compose.local.yml    # 本地开发环境  
│   │   ├── docker-compose.dapr.yml     # 带 Dapr 的开发环境  
│   │   └── docker-compose.prod.yml     # 生产环境  
│   └── Dockerfile.*             # 各服务 Dockerfile  
│  
├── kubernetes/                  # Kubernetes 部署配置  
│   ├── namespaces/  
│   │   └── openbearing.yaml     # 命名空间定义  
│   ├── deployments/             # 服务部署定义  
│   │   ├── identity-deployment.yaml  
│   │   ├── supplier-deployment.yaml  
│   │   ├── demand-deployment.yaml  
│   │   ├── quotation-deployment.yaml  
│   │   └── bearing-deployment.yaml  
│   ├── services/                # Service 定义  
│   ├── configmaps/              # 配置映射  
│   ├── secrets/                 # 密钥定义  
│   └── ingress/                 # 入口配置  
│  
├── terraform/                   # 云资源定义  
│   ├── azure/                   # Azure 资源  
│   ├── aws/                     # AWS 资源  
│   └── aliyun/                  # 阿里云资源  
│  
└── scripts/                     # 运维自动化脚本  
    ├── init-cluster.sh          # 集群初始化  
    ├── deploy-all.sh            # 一键部署  
    └── health-check.sh          # 健康检查  

### 3. 服务层 (`src/services/`)

#### 3.1 身份服务 (`identity/`) - 端口 5001
- **技术栈**: .NET 10, ASP.NET Core Identity, OpenIddict, PostgreSQL 16
- **职责**: 
  - 用户认证和授权
  - OpenID Connect 令牌颁发
  - 用户管理（CRUD）
  - 角色和权限管理
- **API 端点**:
  - `POST /connect/token` - 获取访问令牌
  - `POST /api/users/register` - 用户注册
  - `GET /api/users/{id}` - 获取用户信息

#### 3.2 供应商服务 (`supplier/`) - 端口 5002
- **技术栈**: .NET 10, Dapr, Entity Framework Core, PostgreSQL 16
- **职责**:
  - 供应商信息管理
  - 供应商认证和审核
  - 供应商评分和评级
  - 联系人管理
- **API 端点**:
  - `GET /api/suppliers` - 获取供应商列表
  - `POST /api/suppliers` - 创建供应商
  - `PUT /api/suppliers/{id}/profile` - 更新供应商资料

#### 3.3 需求服务 (`demand/`) - 端口 5003
- **技术栈**: .NET 10, Dapr, Entity Framework Core, PostgreSQL 16
- **职责**:
  - 需求发布和管理
  - 智能匹配算法
  - 实时推送引擎
  - 需求状态跟踪
- **API 端点**:
  - `POST /api/demands` - 发布需求
  - `GET /api/demands` - 获取需求列表
  - `POST /api/demands/{id}/match` - 智能匹配供应商

#### 3.4 报价服务 (`quotation/`) - 端口 5004
- **技术栈**: .NET 10, Dapr, Entity Framework Core, PostgreSQL 16
- **职责**:
  - 报价管理
  - 比价分析
  - 价格趋势分析
  - 报价通知
- **API 端点**:
  - `POST /api/quotations` - 创建报价
  - `GET /api/quotations/demands/{demandId}` - 获取需求报价
  - `GET /api/quotations/analysis` - 价格分析

#### 3.5 轴承服务 (`bearing/`) - 端口 5005
- **技术栈**: .NET 10, Elasticsearch, Dapr, PostgreSQL 16
- **职责**:
  - 轴承信息管理
  - 轴承搜索和分类
  - 规格参数管理
  - 图片和文档管理
- **API 端点**:
  - `GET /api/bearings/search` - 搜索轴承
  - `POST /api/bearings` - 创建轴承记录
  - `GET /api/bearings/{id}/suppliers` - 获取供应商列表

#### 3.6 库存服务 (`inventory/`) - 端口 5006
- **技术栈**: .NET 10, Redis, Dapr, PostgreSQL 16
- **职责**:
  - 供应商库存管理
  - 库存变更记录
  - 库存预警
  - 批次管理
- **API 端点**:
  - `GET /api/inventories` - 获取库存列表
  - `POST /api/inventories` - 添加库存
  - `PUT /api/inventories/{id}/quantity` - 更新库存数量

#### 3.7 通知服务 (`notification/`) - 端口 5007
- **技术栈**: .NET 10, SignalR, Dapr, PostgreSQL 16
- **职责**:
  - 实时消息推送
  - 邮件通知
  - 短信通知
  - 站内信
- **API 端点**:
  - `POST /api/notifications` - 发送通知
  - `GET /api/notifications/users/{userId}` - 获取用户通知
  - `PUT /api/notifications/{id}/read` - 标记为已读

#### 3.8 订单服务 (`order/`) - 端口 5008
- **技术栈**: .NET 10, Dapr, Entity Framework Core, PostgreSQL 16
- **职责**:
  - 订单管理
  - 交付跟踪
  - 履约评估
  - 订单状态管理
- **API 端点**:
  - `POST /api/orders` - 创建订单
  - `GET /api/orders` - 获取订单列表
  - `PUT /api/orders/{id}/status` - 更新订单状态

### 4. Web 前端层 (`src/web/blazor/`)

#### 4.1 Blazor WebAssembly 架构
src/web/blazor/  
├── Client/                    # Blazor WebAssembly 客户端  
│   ├── Pages/                 # 页面组件  
│   │   ├── Index.razor        # 首页  
│   │   ├── Demand.razor       # 需求管理  
│   │   ├── Quotation.razor    # 报价管理  
│   │   └── Inventory.razor    # 库存管理  
│   ├── Components/            # 可复用组件  
│   ├── Services/              # 前端服务  
│   │   ├── ApiService.cs      # API 客户端  
│   │   ├── AuthService.cs     # 认证服务  
│   │   └── SignalRService.cs  # 实时通信  
│   └── Models/                # 数据模型  
├── Server/                    # ASP.NET Core 主机  
│   ├── Controllers/           # API 控制器  
│   └── Program.cs             # 启动配置  
└── Shared/                    # 共享代码  
    ├── Models/                # 共享模型  
    └── Components/            # 共享组件  

#### 4.2 前端技术栈
- **UI 框架**: Blazor WebAssembly (.NET 10)
- **样式**: Bootstrap 5
- **状态管理**: Fluxor
- **认证**: OpenID Connect
- **实时通信**: SignalR
- **HTTP 客户端**: HttpClient + 自定义拦截器

### 5. 移动端层 (`src/mobile/maui/`)

#### 5.1 .NET MAUI 应用
src/mobile/maui/  
├── Platforms/                   # 平台特定代码  
│   ├── Android/  
│   ├── iOS/  
│   └── Windows/  
├── Views/                       # 页面  
│   ├── LoginPage.xaml           # 登录页面  
│   ├── SupplierDashboard.xaml   # 供应商仪表板  
│   ├── DemandPage.xaml          # 需求页面  
│   └── QuotationPage.xaml       # 报价页面  
├── ViewModels/                  # 视图模型  
│   ├── LoginViewModel.cs  
│   ├── SupplierViewModel.cs  
│   ├── DemandViewModel.cs  
│   └── QuotationViewModel.cs  
├── Services/                    # 服务层  
│   ├── ApiService.cs            # API 通信  
│   └── AuthService.cs           # 认证管理  
└── Models/                      # 数据模型  
    ├── DTOs/                    # 数据传输对象  
    └── Entities/                # 实体类  

## 🚀 部署架构

### 开发环境 (Docker Compose)
yaml

version: '3.8'
services:
postgres:
image: postgres:16-alpine
environment:
POSTGRES_DB: openfindbearings
POSTGRES_USER: bearing
POSTGRES_PASSWORD: ${DB_PASSWORD}
ports:
•
"5432:5432"

volumes:
•
postgres_data:/var/lib/postgresql/data
redis:
image: redis:7-alpine
ports:
•
"6379:6379"
identity-service:
build: ./src/services/identity
environment:
•
ASPNETCORE_ENVIRONMENT=Development
•
ConnectionStrings__Default=Host=postgres;Database=identity_db;Username=bearing;Password=${DB_PASSWORD}
ports:
•
"5001:80"
depends_on:
•
postgres
demand-service:
build: ./src/services/demand
environment:
•
ASPNETCORE_ENVIRONMENT=Development
•
ConnectionStrings__Default=Host=postgres;Database=demand_db;Username=bearing;Password=${DB_PASSWORD}
ports:
•
"5003:80"
depends_on:
•
postgres
•
identity-service
blazor-frontend:
build: ./src/web/blazor/Server
ports:
•
"80:80"
depends_on:
•
identity-service
•
demand-service
volumes:
postgres_data:


### 生产环境 (Kubernetes)
yaml

apiVersion: apps/v1
kind: Deployment
metadata:
name: identity-service
namespace: openbearing
spec:
replicas: 3
template:
metadata:
labels:
app: identity
annotations:
dapr.io/enabled: "true"
dapr.io/app-id: "identity"
dapr.io.app-port: "80"
spec:
containers:
•
name: identity
image: your-registry/openfindbearings-identity:latest
ports:
•
containerPort: 80
env:
•
name: ConnectionStrings__Default
value: "Host=postgres;Database=identity_db;Username=bearing;Password=$(DB_PASSWORD)"
•
name: ASPNETCORE_ENVIRONMENT
value: "Production"

## 🔄 核心业务流程

### 1. 需求发布与匹配流程
mermaid

sequenceDiagram
participant S as Supplier
participant B as Blazor前端
participant D as Demand服务
participant I as Inventory服务
participant N as Notification服务

S->>B: 登录系统
B->>D: POST /api/demands (发布需求)
D->>D: 验证需求信息
D->>I: 查询匹配库存
I->>D: 返回有库存供应商
D->>N: 发布需求通知事件
N->>N: 推送通知给匹配供应商
D-->>B: 返回需求创建成功
B-->>S: 显示成功消息

### 2. 报价响应流程
mermaid

sequenceDiagram
participant S as Supplier
participant B as Blazor前端
participant Q as Quotation服务
participant D as Demand服务
participant N as Notification服务

S->>B: 查看需求详情
B->>Q: POST /api/quotations (创建报价)
Q->>Q: 验证报价信息
Q->>D: 更新需求报价状态
D->>N: 发布报价通知事件
N->>N: 推送通知给需求方
Q-->>B: 返回报价成功
B-->>S: 显示报价提交成功

## 🔐 安全架构

### 1. 认证和授权
- **认证协议**: OpenID Connect
- **令牌类型**: JWT (访问令牌 + 刷新令牌)
- **用户类型**: 供应商管理员、供应商用户、系统管理员
- **权限控制**: 基于角色的访问控制 (RBAC)

### 2. 数据安全
- **数据库**: PostgreSQL 16 + 透明数据加密
- **传输加密**: HTTPS/TLS 1.3
- **敏感数据**: 字段级加密
- **审计日志**: 完整操作追踪

### 3. API 安全
- **速率限制**: 防止 API 滥用
- **输入验证**: 模型验证 + 自定义验证
- **SQL 注入防护**: 参数化查询 + ORM
- **XSS 防护**: 输出编码 + CSP 头

## 📊 监控和可观测性

### 1. 应用监控
- **健康检查**: 就绪/存活探针
- **性能指标**: 响应时间、吞吐量、错误率
- **业务指标**: 需求数、报价数、成交率

### 2. 日志管理
- **结构化日志**: Serilog + JSON 格式
- **日志聚合**: ELK Stack 或 Seq
- **日志级别**: 分级日志记录

### 3. 分布式追踪
- **追踪系统**: OpenTelemetry + Jaeger
- **Dapr 集成**: 自动传播追踪上下文
- **服务地图**: 可视化服务依赖


## 📈 扩展性设计

### 1. 数据库设计
- **PostgreSQL 16**: 主从复制、分区表、并行查询
- **连接池**: 动态连接管理
- **读写分离**: 自动路由读写操作
- **分库分表**: 按业务垂直分库

### 2. 缓存策略
- **Redis 集群**: 分布式缓存
- **多级缓存**: 内存缓存 + Redis + 数据库
- **缓存失效**: 主动失效 + TTL 过期

### 3. 消息队列
- **异步处理**: 耗时操作异步化
- **事件驱动**: 服务间解耦
- **重试机制**: 指数退避重试