# 🏗️ OpenFindBearings 系统架构

## 📁 项目目录结构
openfindbearings/  
├── .github/workflows/           # GitHub Actions CI/CD 流水线  
├── dapr/                        # Dapr 配置和组件  
├── deploy/                      # 部署配置（传统 infrastructure 文件夹）  
├── docs/                        # 项目文档  
├── src/                         # 源代码目录  
│   ├── apigateways/             # API 网关实现  
│   ├── mobile/                  # 移动端代码  
│   │   └── maui/                # .NET MAUI 跨平台应用  
│   ├── services/                # 基于 Dapr 的微服务  
│   │   ├── bearing/             # 轴承管理服务  
│   │   ├── identity/            # 认证和用户管理服务  
│   │   ├── inventory/           # 库存管理服务  
│   │   ├── notification/        # 通知服务  
│   │   └── supplier/            # 供应商管理服务  
│   └── web/                     # Web 前端  
│       └── mvc/                 # ASP.NET Core MVC 前端  
└── README.md                    # 项目说明  

## 🎯 架构设计原则

### 1. 基于 Dapr 的微服务架构
- **服务网格**: 使用 Dapr 作为服务网格，简化服务间通信
- **多语言支持**: 不同服务可使用不同技术栈
- **可观测性**: 内置分布式追踪、指标收集
- **弹性设计**: 自动重试、断路器、限流

### 2. 领域驱动设计 (DDD)
- 按业务能力划分服务边界
- 明确的限界上下文
- 领域模型驱动设计

### 3. 云原生设计
- 容器化部署
- 无状态服务设计
- 配置与代码分离
- 健康检查和就绪探针

## 🔧 详细架构说明

### 1. Dapr 配置层 (`dapr/`)

#### 1.1 组件配置

dapr/  
├── components/                  # Dapr 组件定义  
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

#### 3.1 身份服务 (`identity/`)
- **技术栈**: .NET 8, ASP.NET Core Identity, OpenIddict
- **职责**: 
  - 用户认证和授权
  - OpenID Connect 令牌颁发
  - 用户管理（CRUD）
  - 角色和权限管理
- **API 端点**:
  - `POST /connect/token` - 获取访问令牌
  - `POST /api/users/register` - 用户注册
  - `GET /api/users/{id}` - 获取用户信息

#### 3.2 供应商服务 (`supplier/`)
- **技术栈**: .NET 8, Dapr, Entity Framework Core
- **职责**:
  - 供应商信息管理
  - 供应商认证和审核
  - 供应商评分和评级
  - 联系人管理
- **API 端点**:
  - `GET /api/suppliers` - 获取供应商列表
  - `POST /api/suppliers` - 创建供应商
  - `PUT /api/suppliers/{id}/profile` - 更新供应商资料

#### 3.3 轴承服务 (`bearing/`)
- **技术栈**: .NET 8, Elasticsearch, Dapr
- **职责**:
  - 轴承信息管理
  - 轴承搜索和分类
  - 规格参数管理
  - 图片和文档管理
- **API 端点**:
  - `GET /api/bearings/search` - 搜索轴承
  - `POST /api/bearings` - 创建轴承记录
  - `GET /api/bearings/{id}/suppliers` - 获取供应商列表

#### 3.4 库存服务 (`inventory/`)
- **技术栈**: .NET 8, Redis, Dapr
- **职责**:
  - 供应商库存管理
  - 库存变更记录
  - 库存预警
  - 批次管理
- **API 端点**:
  - `GET /api/inventories` - 获取库存列表
  - `POST /api/inventories` - 添加库存
  - `PUT /api/inventories/{id}/quantity` - 更新库存数量

#### 3.5 通知服务 (`notification/`)
- **技术栈**: .NET 8, SignalR, Dapr
- **职责**:
  - 实时消息推送
  - 邮件通知
  - 短信通知
  - 站内信
- **API 端点**:
  - `POST /api/notifications` - 发送通知
  - `GET /api/notifications/users/{userId}` - 获取用户通知
  - `PUT /api/notifications/{id}/read` - 标记为已读

### 4. Web 前端层 (`src/web/mvc/`)

#### 4.1 MVC 架构
src/web/mvc/  
├── Controllers/                 # 控制器层  
│   ├── HomeController.cs        # 首页控制器  
│   ├── SupplierController.cs    # 供应商控制器  
│   ├── BearingController.cs     # 轴承控制器  
│   └── AccountController.cs     # 账户控制器  
├── Views/                       # 视图层  
│   ├── Home/                    # 首页视图  
│   ├── Supplier/                # 供应商视图  
│   ├── Bearing/                 # 轴承视图  
│   └── Account/                 # 账户视图  
├── Models/                      # 模型层  
│   ├── ViewModels/              # 视图模型  
│   └── Entities/                # 数据实体  
└── Services/                    # 服务层  
    ├── ApiService.cs            # API 客户端  
    └── AuthService.cs           # 认证服务  

#### 4.2 前端技术栈
- **UI 框架**: ASP.NET Core MVC
- **样式**: Bootstrap 5
- **JavaScript**: 原生 JS + 必要库
- **认证**: OpenID Connect 隐式流
- **实时通信**: SignalR

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
│   └── InventoryPage.xaml       # 库存页面  
├── ViewModels/                  # 视图模型  
│   ├── LoginViewModel.cs  
│   ├── SupplierViewModel.cs  
│   └── InventoryViewModel.cs  
├── Services/                    # 服务层  
│   ├── ApiService.cs            # API 通信  
│   └── AuthService.cs           # 认证管理  
└── Models/                      # 数据模型  
    ├── DTOs/                    # 数据传输对象  
    └── Entities/                # 实体类  

### 6. API 网关层 (`src/apigateways/`)

#### 6.1 网关架构
- **主网关**: 对外提供统一 API 入口
- **内部网关**: 服务间通信网关
- **功能**:
  - 路由转发
  - 负载均衡
  - 认证和鉴权
  - 限流和熔断
  - 请求/响应转换

## 🚀 部署架构

### 开发环境


使用 Docker Compose
services:

redis:

image: redis:alpine

postgres:

image: postgres:15-alpine

identity-service:

build: ./src/services/identity

ports: ["5001:80"]

supplier-service:

build: ./src/services/supplier

ports: ["5002:80"]

web-mvc:

build: ./src/web/mvc

ports: ["80:80"]

### 生产环境

使用 Kubernetes
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

dapr.io/app-port: "80"

## 🔄 数据流架构

### 1. 用户注册流程

sequenceDiagram
  participant U as 用户
  participant W as Web前端
  participant G as API网关
  participant I as Identity服务
  participant S as Supplier服务
  participant D as Database
  U->>W: 填写注册表单
  W->>G: POST /api/users/register
  G->>I: 转发注册请求
  I->>D: 创建用户记录
  I->>S: 创建供应商记录
  I-->>G: 返回成功
  G-->>W: 返回响应
  W-->>U: 显示成功消息


### 2. 库存更新流程

sequenceDiagram
  participant S as Supplier服务
  participant D as Dapr Sidecar
  participant I as Inventory服务
  participant N as Notification服务
  participant DB as Database
  S->>D: 调用 /api/inventories
  D->>I: 服务调用
  I->>DB: 更新库存
  I->>D: 发布事件 inventory.updated
  D->>N: 事件订阅
  N->>N: 发送通知
  I-->>D: 返回结果
  D-->>S: 操作完成

## 🔐 安全架构

### 1. 认证和授权
- **认证协议**: OpenID Connect
- **令牌类型**: JWT (访问令牌 + 刷新令牌)
- **用户类型**: 供应商、采购商、系统管理员
- **权限控制**: 基于角色的访问控制 (RBAC)

### 2. 网络安全
- **传输安全**: HTTPS/TLS 1.3
- **API 安全**: API 密钥、速率限制
- **防火墙**: 网络隔离、安全组
- **DDoS 防护**: 云服务商防护

### 3. 数据安全
- **加密存储**: 敏感数据加密
- **密钥管理**: 通过 Dapr Secrets 管理
- **审计日志**: 完整操作日志
- **数据备份**: 定期备份和恢复测试

## 📊 监控和可观测性

### 1. 指标收集
- **应用指标**: 请求数、错误率、延迟
- **系统指标**: CPU、内存、磁盘、网络
- **业务指标**: 用户数、订单数、库存数

### 2. 日志管理
- **结构化日志**: JSON 格式日志
- **集中存储**: ELK 或类似方案
- **日志级别**: Debug、Info、Warning、Error

### 3. 分布式追踪
- **追踪系统**: Jaeger 或 Zipkin
- **Dapr 集成**: 自动传播追踪上下文
- **服务地图**: 可视化服务间调用

## 🎯 服务端口配置

| 服务名称 | 端口 | 说明 |
|---------|------|------|
| 身份服务 (identity) | 5001 | 认证和用户管理 |
| 供应商服务 (supplier) | 5002 | 供应商业务管理 |
| 轴承服务 (bearing) | 5003 | 轴承信息管理 |
| 库存服务 (inventory) | 5004 | 库存管理 |
| Web前端 (mvc) | 80 | 供应商门户 |
