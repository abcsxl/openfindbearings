# 次日行动清单 - 2026-01-10

## 今日工作总结

### 已完成工作
1. Notification Service 成功迁移到 Dapr（试点服务）
   - 实现了基于 CloudEvents 的事件订阅机制
   - 集成了 Dapr Pub/Sub 组件
   - 使用 MediatR 实现事件处理模式

2. 创建了 Shared.Dapr 项目
   - Dapr 服务封装（DaprPubSub、DaprServiceInvocation、DaprStateStore）
   - Actor 接口定义（IInventoryActor、IInquiryMatchActor）
   - Actor 状态模型定义

3. 更新了 docker-compose.yml
   - 添加 Dapr Placement Service
   - 为每个微服务配置 Dapr Sidecar 容器
   - 配置 Redis 作为 Pub/Sub 和 State Store

4. 创建了所有 Dapr 配置文件
   - config.yaml：追踪和监控配置
   - pubsub.yaml：Pub/Sub 组件配置
   - statestore.yaml：状态存储配置
   - actorstore.yaml：Actor 存储配置（包含 Actor 实体配置）

---

## 次日行动清单

### 一、Inventory Service Dapr 迁移 + Actor 实现

#### 1.1 项目依赖配置

**步骤 1.1.1：更新 Inventory.Api.csproj**
- 文件路径：`/Users/dr4/WorkSpace/git/openfindbearings/src/services/Inventory.Service/Inventory.Api/Inventory.Api.csproj`
- 参考：`/Users/dr4/WorkSpace/git/openfindbearings/src/services/Notification.Service/Notification.Api/Notification.Api.csproj` (第 10-17 行)
- 需要添加的 NuGet 包：
  - `Dapr.AspNetCore` (v1.13.0)
  - `Dapr.Actors` (v1.13.0) - Actor 支持
- 需要添加的项目引用：
  - `../../shared/Shared.Dapr/Shared.Dapr.csproj`
  - `../../shared/Shared.Domain/Shared.Domain.csproj`

```xml
<ItemGroup>
  <PackageReference Include="Dapr.AspNetCore" Version="1.13.0" />
  <PackageReference Include="Dapr.Actors" Version="1.13.0" />
  <PackageReference Include="MediatR" Version="12.0.0" />
  <PackageReference Include="Swashbuckle.AspNetCore" Version="7.2.0" />
</ItemGroup>

<ItemGroup>
  <ProjectReference Include="..\..\..\shared\Shared.Dapr\Shared.Dapr.csproj" />
  <ProjectReference Include="..\..\..\shared\Shared.Domain\Shared.Domain.csproj" />
  <ProjectReference Include="..\Inventory.Core\Inventory.Core.csproj" />
</ItemGroup>
```

#### 1.2 实现 InventoryActor

**步骤 1.2.1：创建 InventoryActor 类**
- 文件路径：`/Users/dr4/WorkSpace/git/openfindbearings/src/services/Inventory.Service/Inventory.Api/Actors/InventoryActor.cs`
- 接口定义：`/Users/dr4/WorkSpace/git/openfindbearings/src/shared/Shared.Dapr/Actors/IInventoryActor.cs` (第 10-46 行)
- 状态模型：`/Users/dr4/WorkSpace/git/openfindbearings/src/shared/Shared.Dapr/Actors/Models/InventoryActorState.cs` (第 7-125 行)

核心实现要点：
```csharp
using Dapr.Actors;
using Dapr.Actors.Runtime;
using OpenFindBearings.Shared.Dapr.Actors;
using OpenFindBearings.Shared.Dapr.Actors.Models;

namespace OpenFindBearings.Inventory.Api.Actors;

public class InventoryActor : Actor, IInventoryActor
{
    public InventoryActor(ActorService actorService, ActorId actorId)
        : base(actorService, actorId)
    {
    }

    // 实现接口方法
    // GetStateAsync, AddStockAsync, ReduceStockAsync, ReserveStockAsync, etc.
}
```

需要实现的方法：
1. `GetStateAsync()` - 第 15 行：获取库存状态
2. `AddStockAsync(int quantity, string reason)` - 第 20 行：增加库存
3. `ReduceStockAsync(int quantity, string reason)` - 第 25 行：减少库存
4. `ReserveStockAsync(long orderId, int quantity, TimeSpan validFor)` - 第 30 行：预留库存
5. `ConfirmOrderAsync(long orderId)` - 第 35 行：确认订单
6. `CancelReservationAsync(long orderId)` - 第 40 行：取消预留
7. `GetReservationsAsync()` - 第 45 行：获取所有预留

**关键实现细节：**
- 使用 `StateManager.SaveStateAsync()` 保存状态
- 库存预留逻辑需要检查过期时间（参考 `StockReservation.IsExpired()` - 第 93 行）
- 并发安全由 Dapr Actor 框架保证

#### 1.3 创建事件订阅控制器

**步骤 1.3.1：创建 DaprSubscriptionsController**
- 文件路径：`/Users/dr4/WorkSpace/git/openfindbearings/src/services/Inventory.Service/Inventory.Api/Controllers/DaprSubscriptionsController.cs`
- 参考：`/Users/dr4/WorkSpace/git/openfindbearings/src/services/Notification.Service/Notification.Api/Controllers/DaprSubscriptions.cs` (第 1-109 行)

需要订阅的事件：
1. 订单确认事件 (`order-confirmed`) - 调用 Actor 确认库存扣减
2. 订单取消事件 (`order-cancelled`) - 调用 Actor 释放库存预留

```csharp
[Topic("pubsub", "order-confirmed")]
[HttpPost("order-confirmed")]
public async Task<IActionResult> HandleOrderConfirmed([FromBody] OrderConfirmedEvent @event)
{
    // 调用 InventoryActor.ConfirmOrderAsync()
}
```

#### 1.4 更新 Program.cs

**步骤 1.4.1：配置 Dapr 和 Actor 运行时**
- 文件路径：`/Users/dr4/WorkSpace/git/openfindbearings/src/services/Inventory.Service/Inventory.Api/Program.cs`
- 参考：`/Users/dr4/WorkSpace/git/openfindbearings/src/services/Notification.Service/Notification.Api/Program.cs` (第 12-66 行)

关键配置：
```csharp
// 第 12 行：添加 Dapr 客户端
builder.Services.AddDaprClient();

// 添加 Actor 支持
builder.Services.AddActors(options =>
{
    options.Actors.RegisterActor<InventoryActor>();
});

// 第 16 行：添加 MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(InventoryActor).Assembly));

// 第 64 行：配置 HTTP 管道
app.UseCloudEvents();
app.MapControllers();
app.MapSubscribeHandler();

// 配置 Actor 端点
app.MapActorsHandlers();
```

---

### 二、Match Service Dapr 迁移 + Actor 实现

#### 2.1 项目依赖配置

**步骤 2.1.1：更新 Match.Api.csproj**
- 文件路径：`/Users/dr4/WorkSpace/git/openfindbearings/src/services/Match.Service/Match.Api/Match.Api.csproj`
- 参考 Inventory.Api.csproj 的配置
- 需要添加相同的 NuGet 包和项目引用

#### 2.2 实现 InquiryMatchActor

**步骤 2.2.1：创建 InquiryMatchActor 类**
- 文件路径：`/Users/dr4/WorkSpace/git/openfindbearings/src/services/Match.Service/Match.Api/Actors/InquiryMatchActor.cs`
- 接口定义：`/Users/dr4/WorkSpace/git/openfindbearings/src/shared/Shared.Dapr/Actors/IInquiryMatchActor.cs` (第 10-61 行)
- 状态模型：`/Users/dr4/WorkSpace/git/openfindbearings/src/shared/Shared.Dapr/Actors/Models/InquiryMatchActorState.cs` (第 7-295 行)

核心实现要点：
```csharp
using Dapr.Actors;
using Dapr.Actors.Runtime;
using OpenFindBearings.Shared.Dapr.Actors;
using OpenFindBearings.Shared.Dapr.Actors.Models;

namespace OpenFindBearings.Match.Api.Actors;

public class InquiryMatchActor : Actor, IInquiryMatchActor
{
    public InquiryMatchActor(ActorService actorService, ActorId actorId)
        : base(actorService, actorId)
    {
    }

    // 实现接口方法
    // GetStateAsync, StartMatchingAsync, AddCandidateAsync, etc.
}
```

需要实现的方法：
1. `GetStateAsync()` - 第 15 行：获取匹配状态
2. `StartMatchingAsync(InquiryRequest inquiry)` - 第 20 行：开始匹配流程
3. `AddCandidateAsync(MatchCandidate candidate)` - 第 25 行：添加候选供应商
4. `AddCandidatesAsync(List<MatchCandidate> candidates)` - 第 30 行：批量添加候选
5. `GetCandidatesAsync()` - 第 35 行：获取所有候选
6. `SelectBestMatchAsync()` - 第 40 行：选择最佳匹配
7. `CompleteMatchAsync(long supplierId, string reason)` - 第 45 行：完成匹配
8. `CancelMatchAsync(string reason)` - 第 50 行：取消匹配
9. `UpdateProgressAsync(MatchProgress progress)` - 第 55 行：更新进度
10. `SetTimeoutAsync(TimeSpan timeout)` - 第 60 行：设置超时

**关键实现细节：**
- 使用 `RegisterReminder` 实现匹配超时机制
- 匹配进度跟踪（参考 `MatchProgress` - 第 228-259 行）
- 匹配阶段管理（参考 `MatchStage` - 第 264-295 行）

#### 2.3 创建事件订阅控制器

**步骤 2.3.1：创建 DaprSubscriptionsController**
- 文件路径：`/Users/dr4/WorkSpace/git/openfindbearings/src/services/Match.Service/Match.Api/Controllers/DaprSubscriptionsController.cs`
- 参考 Notification Service 的实现

需要订阅的事件：
1. 询价创建事件 (`inquiry-created`) - 创建 InquiryMatchActor 并开始匹配
2. 库存变更事件 (`stock-changed`) - 重新评估匹配候选
3. 供应商响应事件 (`supplier-responded`) - 更新匹配状态

```csharp
[Topic("pubsub", "inquiry-created")]
[HttpPost("inquiry-created")]
public async Task<IActionResult> HandleInquiryCreated([FromBody] InquiryCreatedEvent @event)
{
    // 创建 InquiryMatchActor 实例
    // 调用 StartMatchingAsync()
}
```

#### 2.4 更新 Program.cs

**步骤 2.4.1：配置 Dapr 和 Actor 运行时**
- 文件路径：`/Users/dr4/WorkSpace/git/openfindbearings/src/services/Match.Service/Match.Api/Program.cs`
- 参考 Notification Service 和 Inventory Service 的配置

---

### 三、创建 Actor 服务封装（可选但推荐）

#### 3.1 创建 InventoryActorService

**步骤 3.1.1：创建 Actor 代理服务**
- 文件路径：`/Users/dr4/WorkSpace/git/openfindbearings/src/services/Inventory.Service/Inventory.Core/Services/InventoryActorService.cs`

```csharp
using Dapr.Actors;
using Dapr.Actors.Client;
using OpenFindBearings.Shared.Dapr.Actors;

namespace OpenFindBearings.Inventory.Core.Services;

public interface IInventoryActorService
{
    Task<int> AddStockAsync(long inventoryId, int quantity, string reason);
    Task<bool> ReduceStockAsync(long inventoryId, int quantity, string reason);
    // ... 其他方法
}

public class InventoryActorService : IInventoryActorService
{
    public async Task<int> AddStockAsync(long inventoryId, int quantity, string reason)
    {
        var actorId = new ActorId(inventoryId.ToString());
        var proxy = ActorProxy.Create<IInventoryActor>(actorId, "InventoryActor");
        return await proxy.AddStockAsync(quantity, reason);
    }
}
```

#### 3.2 创建 InquiryMatchActorService

**步骤 3.2.1：创建 Match Actor 代理服务**
- 文件路径：`/Users/dr4/WorkSpace/git/openfindbearings/src/services/Match.Service/Match.Core/Services/InquiryMatchActorService.cs`
- 结构同上

---

### 四、集成测试和验证

#### 4.1 单元测试

**步骤 4.1.1：创建 Actor 单元测试**
- Inventory Actor 测试：测试库存增减、预留逻辑
- Match Actor 测试：测试匹配流程、候选评分逻辑

#### 4.2 集成测试

**步骤 4.2.1：测试端到端流程**
1. 创建询价 → InquiryMatchActor 启动
2. 匹配供应商 → InventoryActor 检查库存
3. 确认订单 → InventoryActor 扣减库存
4. 发布事件 → Notification Service 发送通知

#### 4.3 Docker Compose 验证

**步骤 4.3.1：启动完整环境**
```bash
docker-compose up -d
```

验证清单：
- [ ] 所有服务健康检查通过
- [ ] Dapr Sidecar 连接到 Placement Service
- [ ] Actor 状态存储到 Redis
- [ ] 事件通过 Pub/Sub 正常传递
- [ ] 查看追踪数据（Zipkin：http://localhost:9411）

---

## 关键代码指针速查

### Actor 接口定义
- `IInventoryActor`：`/Users/dr4/WorkSpace/git/openfindbearings/src/shared/Shared.Dapr/Actors/IInventoryActor.cs`
- `IInquiryMatchActor`：`/Users/dr4/WorkSpace/git/openfindbearings/src/shared/Shared.Dapr/Actors/IInquiryMatchActor.cs`

### Actor 状态模型
- `InventoryActorState`：`/Users/dr4/WorkSpace/git/openfindbearings/src/shared/Shared.Dapr/Actors/Models/InventoryActorState.cs`
- `InquiryMatchActorState`：`/Users/dr4/WorkSpace/git/openfindbearings/src/shared/Shared.Dapr/Actors/Models/InquiryMatchActorState.cs`

### 参考实现（Notification Service）
- Program.cs：`/Users/dr4/WorkSpace/git/openfindbearings/src/services/Notification.Service/Notification.Api/Program.cs`
- 事件订阅控制器：`/Users/dr4/WorkSpace/git/openfindbearings/src/services/Notification.Service/Notification.Api/Controllers/DaprSubscriptions.cs`
- 事件处理器：`/Users/dr4/WorkSpace/git/openfindbearings/src/services/Notification.Service/Notification.Core/Events/StockChangedEventHandler.cs`

### Dapr 配置
- Actor 存储配置：`/Users/dr4/WorkSpace/git/openfindbearings/dapr/components/actorstore.yaml` (第 18-39 行包含 Actor 实体配置)
- 主配置文件：`/Users/dr4/WorkSpace/git/openfindbearings/dapr/config.yaml`

### Docker Compose
- Inventory Service Sidecar：`/Users/dr4/WorkSpace/git/openfindbearings/docker-compose.yml` (第 217-237 行)
- Match Service Sidecar：`/Users/dr4/WorkSpace/git/openfindbearings/docker-compose.yml` (第 299-319 行)

---

## 实施优先级

### 高优先级（必须完成）
1. Inventory Service Actor 实现
2. Match Service Actor 实现
3. 事件订阅控制器

### 中优先级（推荐完成）
1. Actor 服务封装
2. 基础集成测试

### 低优先级（后续完成）
1. 完整单元测试覆盖
2. 性能测试
3. 监控和告警配置

---

## 注意事项

### Actor 最佳实践
1. **Actor ID 设计**：使用业务 ID（如 inventoryId、inquiryId）作为 ActorId
2. **状态管理**：只将必要数据存储在 Actor State，大数据存储在数据库
3. **Reminder vs Timer**：Reminder 用于持续性定时任务，Timer 用于临时性任务
4. **并发处理**：Actor 自动保证单线程并发访问
5. **Actor 生命周期**：Actor 会在空闲时自动停用，状态持久化到配置的 State Store

### Dapr 配置要点
1. **Actor 配置**：在 `actorstore.yaml` 中配置了实体生命周期（第 24-36 行）
2. **Pub/Sub**：使用 Redis 作为消息代理，支持 CloudEvents 协议
3. **追踪**：Zipkin 已配置，100% 采样率（开发环境）

### 与现有系统集成
1. **事件发布**：使用 `IDaprPubSub` 发布事件（`/Users/dr4/WorkSpace/git/openfindbearings/src/shared/Shared.Dapr/Services/DaprPubSub.cs`）
2. **服务调用**：使用 Dapr Service Invocation 进行同步调用
3. **状态存储**：Actor 状态自动存储到 Redis（通过 actorstore 组件）

---

## 预估工作量

- Inventory Service Dapr 迁移：2-3 小时
- Match Service Dapr 迁移：3-4 小时
- Actor 服务封装：1-2 小时
- 集成测试：1-2 小时

**总计：7-11 小时**

---

## 验收标准

### 功能验收
- [ ] Inventory Actor 能正确处理库存增减
- [ ] Inventory Actor 能正确处理库存预留和确认
- [ ] Match Actor 能完成完整匹配流程
- [ ] 事件能正确触发 Actor 方法调用
- [ ] Actor 状态能正确持久化和恢复

### 技术验收
- [ ] 所有服务能正常启动并连接到 Dapr Sidecar
- [ ] Actor 能正常注册和通信
- [ ] 事件能通过 Pub/Sub 正常传递
- [ ] 追踪数据能在 Zipkin 中查看
- [ ] 没有内存泄漏和性能问题

---

生成时间：2026-01-10
基于：Notification Service Dapr 迁移经验
