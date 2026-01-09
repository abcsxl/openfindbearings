# OpenFindBearings - 每日反思与行动清单

**日期**: 2025-01-09
**项目**: OpenFindBearings 轴承交易平台
**当前阶段**: 微服务架构搭建完成，开始实现核心服务

---

## 今日完成工作总结

### 1. 项目结构搭建 ✅
- 创建了完整的 .NET 10 微服务解决方案（37个项目）
- 8个业务微服务 + 1个API网关 + 4个共享库 + 2个测试项目
- 配置了 Docker Compose 容器化部署环境

### 2. 共享基础设施代码 ✅
- **领域事件**: `DomainEvent` 基类和集成事件定义
- **实体基类**: `Entity<TId>` 支持领域事件
- **值对象**: `Email`、`PhoneNumber` 等值对象
- **响应模型**: `ApiResponse<T>`、`PagedResponse<T>` 统一API响应格式
- **消息总线**: MassTransit + RabbitMQ 配置扩展

### 3. 数据库设计 ✅
- 7个业务数据库完整表结构设计
- 40+张表，包含索引、约束、默认值
- 支持全文搜索、JSONB存储、生成列等高级特性

### 4. 设计文档 ✅
- 架构概览文档
- API设计文档（30+个端点）
- 数据库设计文档（完整数据字典）

---

## 次日行动清单（优先级排序）

### 🔥 P0 - 核心认证功能（预计 4-5 小时）

#### 1. 实现 Auth Service 领域层（1.5小时）

**文件路径**: `/Users/dr4/WorkSpace/git/openfindbearings/src/services/Auth.Service/Auth.Domain/`

**任务清单**:
- [ ] 创建 `Entities/User.cs` - 用户实体
  ```csharp
  // 需要包含的字段：Id, Username, PhoneNumber, Email, PasswordHash,
  // WechatOpenId, AvatarUrl, FullName, IsActive, IsPhoneVerified,
  // LastLoginAt, CreatedAt, UpdatedAt
  ```
- [ ] 创建 `Entities/Role.cs` - 角色实体
- [ ] 创建 `Entities/UserRole.cs` - 用户角色关联
- [ ] 创建 `Entities/RefreshToken.cs` - 刷新令牌实体
- [ ] 创建 `ValueObjects/Password.cs` - 密码值对象（哈希、验证）
- [ ] 创建 `Interfaces/IUserRepository.cs` - 用户仓储接口
- [ ] 创建 `Interfaces/IRefreshTokenRepository.cs` - 令牌仓储接口

**参考文件**:
- 实体基类: `/Users/dr4/WorkSpace/git/openfindbearings/src/shared/Shared.Domain/Entities/Entity.cs`
- 值对象基类: `/Users/dr4/WorkSpace/git/openfindbearings/src/shared/Shared.Domain/ValueObjects/ValueObject.cs`
- 数据库表结构: `/Users/dr4/WorkSpace/git/openfindbearings/infrastructure/scripts/init-db.sql` (第34-70行)

---

#### 2. 实现 Auth Service 基础设施层（1.5小时）

**文件路径**: `/Users/dr4/WorkSpace/git/openfindbearings/src/services/Auth.Service/Auth.Infrastructure/`

**任务清单**:
- [ ] 添加 NuGet 包依赖
  - `Npgsql.EntityFrameworkCore.PostgreSQL` (EF Core PostgreSQL)
  - `Microsoft.EntityFrameworkCore.Design`
  - `OpenIddict.EntityFrameworkCore` (如果使用 EF Core 存储)
- [ ] 创建 `Data/AuthDbContext.cs` - EF Core 数据库上下文
  ```csharp
  // DbSets: Users, Roles, UserRoles, RefreshTokens
  // 配置实体关系、索引、约束
  ```
- [ ] 创建 `Repositories/UserRepository.cs` - 用户仓储实现
- [ ] 创建 `Repositories/RefreshTokenRepository.cs` - 令牌仓储实现
- [ ] 创建 `Migrations/` - EF Core 迁移（初始迁移）
- [ ] 创建 `Extensions/ServiceCollectionExtensions.cs` - DI 配置

**参考文件**:
- 连接字符串配置: `/Users/dr4/WorkSpace/git/openfindbearings/docker-compose.yml` (第57行)
- 数据库表结构: `/Users/dr4/WorkSpace/git/openfindbearings/infrastructure/scripts/init-db.sql` (第22-85行)

---

#### 3. 实现 Auth Service 核心业务层（1.5小时）

**文件路径**: `/Users/dr4/WorkSpace/git/openfindbearings/src/services/Auth.Service/Auth.Core/`

**任务清单**:
- [ ] 创建 `DTOs/RegisterRequest.cs` - 注册请求DTO
- [ ] 创建 `DTOs/LoginRequest.cs` - 登录请求DTO
- [ ] 创建 `DTOs/LoginResponse.cs` - 登录响应DTO（含token）
- [ ] 创建 `DTOs/RefreshTokenRequest.cs` - 刷新令牌请求DTO
- [ ] 创建 `DTOs/UserDto.cs` - 用户DTO
- [ ] 创建 `Interfaces/IAuthService.cs` - 认证服务接口
  ```csharp
  // 方法签名:
  // Task<LoginResponse> LoginAsync(LoginRequest request)
  // Task<UserDto> RegisterAsync(RegisterRequest request)
  // Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request)
  // Task<bool> ValidateTokenAsync(string token)
  ```
- [ ] 创建 `Services/JwtTokenService.cs` - JWT令牌服务
  ```csharp
  // 方法签名:
  // string GenerateAccessToken(User user, IEnumerable<string> roles)
  // string GenerateRefreshToken()
  // ClaimsPrincipal? ValidateToken(string token)
  ```
- [ ] 创建 `Services/AuthService.cs` - 认证服务实现
  ```csharp
  // 实现逻辑:
  // - 用户名密码验证
  // - 密码哈希（BCrypt）
  // - JWT令牌生成
  // - 刷新令牌管理
  // - 用户注册（默认分配Supplier角色）
  ```

**参考文件**:
- API设计文档: `/Users/dr4/WorkSpace/git/openfindbearings/docs/design/02-api-design.md` (第33-99行)
- 响应模型: `/Users/dr4/WorkSpace/git/openfindbearings/src/shared/Shared.DTOs/Models/ApiResponse.cs`

---

#### 4. 实现 Auth Service API层（1小时）

**文件路径**: `/Users/dr4/WorkSpace/git/openfindbearings/src/services/Auth.Service/Auth.Api/`

**任务清单**:
- [ ] 更新 `Auth.Api.csproj` 添加 NuGet 包
  - `Microsoft.AspNetCore.Authentication.JwtBearer`
  - `Microsoft.AspNetCore.Authentication.OpenIdConnect` (如果需要 OpenIddict)
  - `Swashbuckle.AspNetCore` (Swagger)
- [ ] 创建 `Controllers/AuthController.cs` - 认证控制器
  ```csharp
  // 端点:
  // POST /api/auth/register
  // POST /api/auth/login
  // POST /api/auth/refresh-token
  // POST /api/auth/logout
  ```
- [ ] 创建 `Controllers/UsersController.cs` - 用户管理控制器
  ```csharp
  // 端点:
  // GET /api/users/me
  // PUT /api/users/me
  ```
- [ ] 更新 `Program.cs` 配置服务
  ```csharp
  // 配置项:
  // - PostgreSQL 连接
  // - JWT 认证
  // - Swagger/OpenAPI
  // - CORS
  // - 依赖注入
  ```
- [ ] 创建 `Configuration/appsettings.json` - 应用配置
- [ ] 创建 `Configuration/appsettings.Development.json` - 开发环境配置

**参考文件**:
- API设计文档: `/Users/dr4/WorkSpace/git/openfindbearings/docs/design/02-api-design.md` (第33-99行)
- 现有Program.cs: `/Users/dr4/WorkSpace/git/openfindbearings/src/services/Auth.Service/Auth.Api/Program.cs`

---

### 📦 P1 - 配置与依赖（预计 1-2 小时）

#### 5. 配置项目依赖（1小时）

**任务清单**:
- [ ] 更新 `/Users/dr4/WorkSpace/git/openfindbearings/src/services/Auth.Service/Auth.Infrastructure/Auth.Infrastructure.csproj`
  ```xml
  <!-- 添加依赖 -->
  <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.0" />
  <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="9.0.0" />
  ```
- [ ] 更新 `/Users/dr4/WorkSpace/git/openfindbearings/src/services/Auth.Service/Auth.Core/Auth.Core.csproj`
  ```xml
  <PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
  <PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.0.0" />
  ```
- [ ] 更新 `/Users/dr4/WorkSpace/git/openfindbearings/src/services/Auth.Service/Auth.Api/Auth.Api.csproj`
  ```xml
  <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.0" />
  <PackageReference Include="Swashbuckle.AspNetCore" Version="7.0.0" />
  <ProjectReference Include="..\..\..\shared\Shared.DTOs\Shared.DTOs.csproj" />
  ```
- [ ] 添加项目引用
  - Auth.Api → Auth.Core
  - Auth.Core → Auth.Domain
  - Auth.Infrastructure → Auth.Domain

---

#### 6. 数据库初始化（0.5小时）

**任务清单**:
- [ ] 创建 `/Users/dr4/WorkSpace/git/openfindbearings/src/services/Auth.Service/Auth.Infrastructure/DesignTimeDbContextFactory.cs`
  ```csharp
  // 用于 EF Core 迁移的工厂类
  ```
- [ ] 执行初始迁移
  ```bash
  cd /Users/dr4/WorkSpace/git/openfindbearings/src/services/Auth.Service/Auth.Infrastructure
  dotnet ef migrations add InitialCreate --startup-project ../Auth.Api
  dotnet ef database update --startup-project ../Auth.Api
  ```
- [ ] 验证数据库表结构是否与设计文档一致

**参考文件**:
- 数据库设计: `/Users/dr4/WorkSpace/git/openfindbearings/infrastructure/scripts/init-db.sql` (第22-85行)

---

### 🧪 P2 - 测试与验证（预计 1 小时）

#### 7. 单元测试（0.5小时）

**文件路径**: `/Users/dr4/WorkSpace/git/openfindbearings/tests/Unit.Tests/`

**任务清单**:
- [ ] 创建 `AuthServiceTests.cs` - 认证服务测试
  ```csharp
  // 测试用例:
  // - Login_ValidCredentials_ReturnsToken
  // - Login_InvalidCredentials_ReturnsNull
  // - Register_NewUser_ReturnsUserDto
  // - Register_DuplicateUsername_ThrowsException
  // - RefreshToken_ValidToken_ReturnsNewToken
  ```
- [ ] 创建 `PasswordHasherTests.cs` - 密码哈希测试
- [ ] 创建 `JwtTokenServiceTests.cs` - JWT令牌测试

---

#### 8. 集成测试（0.5小时）

**文件路径**: `/Users/dr4/WorkSpace/git/openfindbearings/tests/Integration.Tests/`

**任务清单**:
- [ ] 创建 `AuthApiTests.cs` - Auth API 集成测试
  ```csharp
  // 测试端点:
  // - POST /api/auth/register
  // - POST /api/auth/login
  // - POST /api/auth/refresh-token
  // - GET /api/users/me (需要认证)
  ```
- [ ] 配置测试数据库（使用 Testcontainers 或内存数据库）
- [ ] 编写测试fixture配置

---

### 📚 P3 - 文档与优化（可选，0.5小时）

#### 9. 更新文档

**任务清单**:
- [ ] 更新 `/Users/dr4/WorkSpace/git/openfindbearings/docs/design/README.md` - 标记Auth Service完成状态
- [ ] 创建 `/Users/dr4/WorkSpace/git/openfindbearings/docs/api/auth-service.md` - Auth Service API文档
- [ ] 更新 `/Users/dr4/WorkSpace/git/openfindbearings/readme.md` - 添加运行说明

---

## 代码指针索引

### 领域层
- **用户实体**: `/Users/dr4/WorkSpace/git/openfindbearings/src/services/Auth.Service/Auth.Domain/Entities/User.cs`
- **角色实体**: `/Users/dr4/WorkSpace/git/openfindbearings/src/services/Auth.Service/Auth.Domain/Entities/Role.cs`
- **刷新令牌实体**: `/Users/dr4/WorkSpace/git/openfindbearings/src/services/Auth.Service/Auth.Domain/Entities/RefreshToken.cs`
- **用户仓储接口**: `/Users/dr4/WorkSpace/git/openfindbearings/src/services/Auth.Service/Auth.Domain/Interfaces/IUserRepository.cs`

### 基础设施层
- **数据库上下文**: `/Users/dr4/WorkSpace/git/openfindbearings/src/services/Auth.Service/Auth.Infrastructure/Data/AuthDbContext.cs`
- **用户仓储实现**: `/Users/dr4/WorkSpace/git/openfindbearings/src/services/Auth.Service/Auth.Infrastructure/Repositories/UserRepository.cs`
- **DI配置**: `/Users/dr4/WorkSpace/git/openfindbearings/src/services/Auth.Service/Auth.Infrastructure/Extensions/ServiceCollectionExtensions.cs`

### 核心业务层
- **认证服务**: `/Users/dr4/WorkSpace/git/openfindbearings/src/services/Auth.Service/Auth.Core/Services/AuthService.cs`
- **JWT令牌服务**: `/Users/dr4/WorkSpace/git/openfindbearings/src/services/Auth.Service/Auth.Core/Services/JwtTokenService.cs`
- **DTOs**: `/Users/dr4/WorkSpace/git/openfindbearings/src/services/Auth.Service/Auth.Core/DTOs/`

### API层
- **认证控制器**: `/Users/dr4/WorkSpace/git/openfindbearings/src/services/Auth.Service/Auth.Api/Controllers/AuthController.cs`
- **用户控制器**: `/Users/dr4/WorkSpace/git/openfindbearings/src/services/Auth.Service/Auth.Api/Controllers/UsersController.cs`
- **启动配置**: `/Users/dr4/WorkSpace/git/openfindbearings/src/services/Auth.Service/Auth.Api/Program.cs`

### 共享库
- **实体基类**: `/Users/dr4/WorkSpace/git/openfindbearings/src/shared/Shared.Domain/Entities/Entity.cs`
- **值对象基类**: `/Users/dr4/WorkSpace/git/openfindbearings/src/shared/Shared.Domain/ValueObjects/ValueObject.cs`
- **领域事件**: `/Users/dr4/WorkSpace/git/openfindbearings/src/shared/Shared.Domain/Events/DomainEvent.cs`
- **集成事件**: `/Users/dr4/WorkSpace/git/openfindbearings/src/shared/Shared.Domain/Events/IntegrationEvents.cs`
- **响应模型**: `/Users/dr4/WorkSpace/git/openfindbearings/src/shared/Shared.DTOs/Models/ApiResponse.cs`
- **分页响应**: `/Users/dr4/WorkSpace/git/openfindbearings/src/shared/Shared.DTOs/Models/PagedResponse.cs`
- **消息总线配置**: `/Users/dr4/WorkSpace/git/openfindbearings/src/shared/Shared.Infrastructure/Messaging/MassTransitConfiguration.cs`

### 配置文件
- **Docker Compose**: `/Users/dr4/WorkSpace/git/openfindbearings/docker-compose.yml`
- **数据库脚本**: `/Users/dr4/WorkSpace/git/openfindbearings/infrastructure/scripts/init-db.sql`
- **环境变量示例**: `/Users/dr4/WorkSpace/git/openfindbearings/.env.example`

### 设计文档
- **架构概览**: `/Users/dr4/WorkSpace/git/openfindbearings/docs/design/01-architecture-overview.md`
- **API设计**: `/Users/dr4/WorkSpace/git/openfindbearings/docs/design/02-api-design.md`
- **数据库设计**: `/Users/dr4/WorkSpace/git/openfindbearings/docs/design/03-database-design.md`

---

## 技术要点提醒

### OpenIddict 配置（如果使用）
```csharp
// Program.cs
builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
               .UseDbContext<AuthDbContext>();
    })
    .AddServer(options =>
    {
        options.SetTokenEndpointUris("/connect/token")
               .AllowPasswordFlow()
               .AllowRefreshTokenFlow()
               .AddSigningCertificate(certificate);
    });
```

### JWT 配置（推荐使用，更简单）
```json
// appsettings.json
{
  "Jwt": {
    "SigningKey": "YourSuperSecretKeyForJWT1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ",
    "Issuer": "http://localhost:5000",
    "Audience": "openfindbearings",
    "ExpirationMinutes": 60,
    "RefreshExpirationDays": 7
  }
}
```

### 密码哈希（使用 BCrypt）
```csharp
using BCrypt.Net;

// 哈希密码
string hash = BCrypt.Net.BCrypt.HashPassword(plainPassword);

// 验证密码
bool isValid = BCrypt.Net.BCrypt.Verify(plainPassword, hash);
```

### EF Core PostgreSQL 配置
```csharp
options.UseNpgsql(connectionString, npgsqlOptions =>
{
    npgsqlOptions.EnableRetryOnFailure(
        maxRetryCount: 3,
        maxRetryDelay: TimeSpan.FromSeconds(5),
        errorCodesToAdd: null);
});
```

---

## 预估工作量

| 优先级 | 任务模块 | 预估时间 | 依赖关系 |
|--------|----------|----------|----------|
| P0 | Auth Service 领域层 | 1.5h | 无 |
| P0 | Auth Service 基础设施层 | 1.5h | 领域层 |
| P0 | Auth Service 核心业务层 | 1.5h | 领域层 + 基础设施层 |
| P0 | Auth Service API层 | 1h | 核心业务层 |
| P1 | 配置项目依赖 | 1h | 无 |
| P1 | 数据库初始化 | 0.5h | 基础设施层 |
| P2 | 单元测试 | 0.5h | 核心业务层 |
| P2 | 集成测试 | 0.5h | API层 |
| P3 | 文档更新 | 0.5h | 所有模块完成 |
| **总计** | | **8.5h** | |

---

## 次日目标

### 核心目标（必须完成）
1. ✅ Auth Service 完整实现（领域层 + 基础设施层 + 核心层 + API层）
2. ✅ 用户注册、登录、刷新令牌功能可用
3. ✅ 数据库迁移成功，表结构正确
4. ✅ 可以通过 Postman/Swagger 测试所有认证端点

### 附加目标（尽量完成）
1. 基础单元测试覆盖
2. Swagger UI 配置完成
3. Docker 容器运行测试

---

## 下一步预览

完成 Auth Service 后，按优先级顺序实现：
1. **User Service** - 用户/企业管理（依赖 Auth Service）
2. **Bearing Service** - 轴承主数据（基础服务，无依赖）
3. **Inventory Service** - 库存管理（依赖 User + Bearing）
4. **API Gateway** - 网关路由配置（依赖所有服务）

---

## 备注

- 所有代码使用中文注释
- 遵循 Clean Architecture 原则
- 使用共享库的通用组件（不要重复造轮子）
- 数据库表结构已设计完成，直接参考 `init-db.sql`
- API端点已设计完成，参考 `02-api-design.md`
- 遇到问题优先查看设计文档，再进行技术选型

**次日优先级**: 完成认证授权服务，为其他服务提供统一的身份认证基础。
