# Dapr 实施指南

本文档提供 OpenFindBearings 轴承交易平台迁移到 Dapr 架构的详细实施指南。

## 目录

- [环境准备](#环境准备)
- [Dapr 配置文件](#dapr-配置文件)
- [项目结构调整](#项目结构调整)
- [代码迁移指南](#代码迁移指南)
- [Docker Compose 配置](#docker-compose-配置)
- [Actor 实现](#actor-实现)

## 环境准备

### 安装 Dapr CLI

```bash
# macOS
brew tap dapr/tap
brew install dapr-cli

# 验证安装
dapr --version

# 初始化 Dapr (本地开发)
dapr init

# 验证 Dapr 状态
dapr status
```

### Docker 安装 Dapr Sidecar

生产环境使用 Docker 容器化的 Dapr Sidecar，无需在容器内安装 Dapr CLI。

## Dapr 配置文件

### 目录结构

```
dapr/
├── config.yaml                # 全局配置
└── components/                # 组件配置
    ├── pubsub.yaml           # Pub/Sub 组件
    ├── statestore.yaml       # 状态存储组件
    ├── actorstore.yaml       # Actor 状态存储
    └── secretstore.yaml      # Secret 存储
```

### 全局配置 (dapr/config.yaml)

```yaml
apiVersion: dapr.io/v1alpha1
kind: Configuration
metadata:
  name: appconfig
spec:
  tracing:
    samplingRate: "1"  # 100% 采样率
    zipkin:
      endpointAddress: "http://zipkin:9411/api/v2/spans"
  metrics:
    enabled: true
  features:
    - name: Actor.Reminders.Balance
      enabled: true
```

### Pub/Sub 组件 (dapr/components/pubsub.yaml)

```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: pubsub
spec:
  type: pubsub.redis
  version: v1
  metadata:
    - name: redisHost
      value: redis:6379
    - name: redisPassword
      value: ${REDIS_PASSWORD:-Redis123!}
    - name: consumerID
      value: "openfindbearings"
```

### 状态存储组件 (dapr/components/statestore.yaml)

```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: statestore
spec:
  type: state.redis
  version: v1
  metadata:
    - name: redisHost
      value: redis:6379
    - name: redisPassword
      value: ${REDIS_PASSWORD:-Redis123!}
    - name: keyPrefix
      value: none
```

### Actor 状态存储 (dapr/components/actorstore.yaml)

```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: actorstore
spec:
  type: state.redis
  version: v1
  metadata:
    - name: redisHost
      value: redis:6379
    - name: redisPassword
      value: ${REDIS_PASSWORD:-Redis123!}
    - name: actorStateStore
      value: "true"
    - name: keyPrefix
      value: "actors"
```

### Secret 存储组件 (dapr/components/secretstore.yaml)

```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: secretstore
spec:
  type: secretstores.env.local
  version: v1
```

## 项目结构调整

### 新增项目

```
src/shared/Shared.Dapr/
├── Shared.Dapr.csproj
├── Services/
│   ├── DaprServiceInvocation.cs   # 服务调用封装
│   ├── DaprPubSub.cs              # 发布订阅封装
│   └── DaprStateStore.cs          # 状态存储封装
└── Actors/
    ├── IInventoryActor.cs         # 库存 Actor 接口
    ├── IInquiryMatchActor.cs      # 匹配 Actor 接口
    └── Models/                    # Actor 状态模型
```

### 移除内容

删除 `src/shared/Shared.Infrastructure/Messaging/MassTransitConfiguration.cs`

## 代码迁移指南

### 服务调用

**之前 (直接 HTTP):**
```csharp
public class BearingServiceClient
{
    private readonly HttpClient _httpClient;

    public async Task<BearingDto?> GetBearingAsync(long id)
    {
        return await _httpClient.GetFromJsonAsync<BearingDto>(
            $"http://bearing-service/api/bearings/{id}");
    }
}
```

**之后 (Dapr Service Invocation):**
```csharp
public class BearingServiceClient
{
    private readonly DaprClient _daprClient;

    public async Task<BearingDto?> GetBearingAsync(long id)
    {
        return await _daprClient.InvokeMethodAsync<BearingDto>(
            HttpMethod.Get,
            "bearing-service",  // 服务 ID
            $"api/bearings/{id}");
    }
}
```

### 发布订阅

**之前 (MassTransit):**
```csharp
// 发布
await _publishEndpoint.Publish(new StockChangedEvent { ... });

// 消费者
public class StockChangedConsumer : IConsumer<StockChangedEvent>
{
    public async Task Consume(ConsumeContext<StockChangedEvent> context)
    {
        var @event = context.Message;
        // 处理事件...
    }
}
```

**之后 (Dapr Pub/Sub):**
```csharp
// 发布
await _daprClient.PublishEventAsync(
    "pubsub",
    "stock-changed",
    new StockChangedEvent { ... });

// 订阅
[HttpPost("stock-changed")]
[Topic("pubsub", "stock-changed")]
public async Task HandleStockChanged([FromBody] StockChangedEvent @event)
{
    // 处理事件...
}
```

### Program.cs 配置

**每个服务的 Program.cs 需要添加：**
```csharp
// 添加 Dapr 客户端
builder.Services.AddDaprClient();

// 添加控制器并启用 Dapr 订阅支持
builder.Services.AddControllers()
    .AddDapr();

var app = builder.Build();

// 添加订阅端点
app.MapSubscribeHandler();

app.MapControllers();
```

### NuGet 包变更

**移除：**
```xml
<PackageReference Include="MassTransit" Version="8.2.0" />
<PackageReference Include="MassTransit.RabbitMQ" Version="8.2.0" />
```

**新增：**
```xml
<PackageReference Include="Dapr.AspNetCore" Version="1.13.0" />
<PackageReference Include="Dapr.Client" Version="1.13.0" />
```

## Docker Compose 配置

### 服务配置示例

每个服务需要配置 Dapr Sidecar 容器：

```yaml
services:
  # 库存服务
  inventory-service:
    build:
      context: ./src/services/Inventory.Service/Inventory.Api
      dockerfile: Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:5004
    ports:
      - "5004:5004"
    volumes:
      - ./src/services/Inventory.Service/Inventory.Api:/app
    networks:
      - openfindbearings-network

  # 库存服务 Dapr Sidecar
  inventory-service-dapr:
    image: daprio/daprd:latest
    command: [
      "./daprd",
      "--app-id", "inventory-service",
      "--app-port", "5004",
      "--dapr-http-port", "3504",
      "--dapr-grpc-port", "50004",
      "--config", "/config/config.yaml",
      "--components-path", "/components",
      "--placement-host-address", "placement:50006"
    ]
    volumes:
      - ./dapr:/config
      - ./dapr/components:/components
    depends_on:
      - inventory-service
      - redis
      - zipkin
      - placement
    ports:
      - "3504:3504"
      - "50004:50004"
    networks:
      - openfindbearings-network

  # Dapr Placement Service (用于 Actors)
  placement:
    image: daprio/placement:latest
    command: ["./placement", "-port", "50006"]
    ports:
      - "50006:50006"
    networks:
      - openfindbearings-network

  # Zipkin 分布式追踪
  zipkin:
    image: openzipkin/zipkin
    ports:
      - "9411:9411"
    networks:
      - openfindbearings-network
```

## Actor 实现

### 库存 Actor (Inventory.Service)

**Actor 接口定义 (Shared.Dapr):**
```csharp
public interface IInventoryActor : IActor
{
    Task<InventoryActorState> GetStateAsync();
    Task<int> AddStockAsync(int quantity, string reason);
    Task<bool> ReserveStockAsync(long orderId, int quantity, TimeSpan validFor);
    Task<bool> ConfirmOrderAsync(long orderId);
    Task<bool> CancelReservationAsync(long orderId);
}
```

**Actor 实现 (Inventory.Api/Actors):**
```csharp
[Actor(TypeName = "InventoryActor")]
public class InventoryActor : Actor, IInventoryActor
{
    private const string StateKeyName = "inventory_state";
    private readonly ILogger<InventoryActor> _logger;

    public async Task<bool> ReserveStockAsync(long orderId, int quantity, TimeSpan validFor)
    {
        var state = await GetOrCreateStateAsync();

        if (state.AvailableQuantity < quantity)
            return false;

        state.AvailableQuantity -= quantity;
        state.ReservedQuantity += quantity;
        state.LastUpdated = DateTime.UtcNow;

        await SaveStateAsync(state);

        // 注册提醒，检查预留过期
        await RegisterReminderAsync(
            $"ReservationExpiry-{orderId}",
            null,
            validFor - TimeSpan.FromMinutes(5),
            TimeSpan.FromMinutes(1));

        return true;
    }

    protected override async Task ReceiveReminderAsync(
        string reminderName,
        byte[] state,
        TimeSpan dueTime,
        TimeSpan period)
    {
        if (reminderName.StartsWith("ReservationExpiry-"))
        {
            // 处理预留过期
            await CleanExpiredReservationsAsync();
        }
    }
}
```

**Actor 调用 (Controller):**
```csharp
[ApiController]
[Route("api/inventory")]
public class InventoryController : ControllerBase
{
    private readonly IActorProxyFactory _actorProxyFactory;

    [HttpPost("{id}/reserve")]
    public async Task<IActionResult> ReserveStock(long id, [FromBody] ReserveStockRequest request)
    {
        var actorId = new ActorId(id.ToString());
        var proxy = _actorProxyFactory.CreateActorProxy<IInventoryActor>(
            actorId,
            "InventoryActor");

        var success = await proxy.ReserveStockAsync(
            request.OrderId,
            request.Quantity,
            TimeSpan.FromMinutes(30));

        return Ok(new { success });
    }
}
```

## 验证清单

### Dapr 环境检查

```bash
# 检查 Dapr 版本
dapr --version

# 检查 Dapr 状态
dapr status

# 检查 Dapr 组件
dapr components -k
```

### 服务验证

- [ ] 所有服务可以正常启动
- [ ] Dapr Sidecar 连接正常
- [ ] 服务间调用通过 Dapr 成功
- [ ] 消息发布订阅正常工作
- [ ] Actor 调用正常
- [ ] 分布式追踪在 Zipkin 可见 (http://localhost:9411)

## 迁移时间估算

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

## 参考链接

- [Dapr 官方文档](https://docs.dapr.io/)
- [Dapr .NET SDK](https://github.com/dapr/dotnet-sdk)
- [Dapr Actors](https://docs.dapr.io/developing-applications/building-blocks/actors/)
- [Dapr Pub/Sub](https://docs.dapr.io/developing-applications/building-blocks/pubsub/)
