using BearingApi.Data;
using BearingApi.Models.Entities;
using BearingApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 添加配置
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// 添加数据库上下文
builder.Services.AddDbContext<BearingDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Host=localhost;Database=bearingdb;Username=postgres;Password=postgres";

    options.UseNpgsql(connectionString);

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// 添加服务
builder.Services.AddScoped<IBearingRepository, BearingRepository>();
builder.Services.AddScoped<IBearingService, BearingService>();

// 添加控制器
builder.Services.AddControllers();

// 添加 Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Bearing API",
        Version = "v1",
        Description = "轴承管理API"
    });

    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});

// 简化认证配置
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "BearingApi",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "BearingApi",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKeyForJwtTokenSigning"))
        };
    });

builder.Services.AddAuthorization();

// 添加CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// 配置中间件
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bearing API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// 健康检查端点
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }));

// 初始化数据库
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BearingDbContext>();
    await context.Database.EnsureCreatedAsync();

    // 开发环境：添加示例数据
    if (app.Environment.IsDevelopment())
    {
        await SeedSampleDataAsync(context);
    }
}

app.Run();

// 示例数据种子方法
//async Task SeedSampleDataAsync(BearingDbContext context)
//{
//    if (await context.Bearings.AnyAsync()) return;

//    var sampleBearings = new[]
//    {
//        new Bearing
//        {
//            BearingNumber = "6204-2RS",
//            DisplayName = "6204-2RS 深沟球轴承",
//            Type = BearingType.DeepGrooveBallBearing,
//            Brand = "SKF",
//            InnerDiameter = 20,
//            OuterDiameter = 47,
//            Width = 14,
//            Status = BearingStatus.Active,
//            CreatedAt = DateTime.UtcNow,
//            UpdatedAt = DateTime.UtcNow
//        },
//        new Bearing
//        {
//            BearingNumber = "6305",
//            DisplayName = "6305 深沟球轴承",
//            Type = BearingType.DeepGrooveBallBearing,
//            Brand = "NSK",
//            InnerDiameter = 25,
//            OuterDiameter = 62,
//            Width = 17,
//            Status = BearingStatus.Active,
//            CreatedAt = DateTime.UtcNow,
//            UpdatedAt = DateTime.UtcNow
//        }
//    };

//    context.Bearings.AddRange(sampleBearings);
//    await context.SaveChangesAsync();
//}

//async Task SeedSampleDataAsync(BearingDbContext context)
//{
//    if (await context.Bearings.AnyAsync()) return;

//    var sampleBearings = new List<Bearing>
//    {
//        new()
//        {
//            BearingNumber = "6204-2RS",
//            DisplayName = "6204-2RS 深沟球轴承",
//            Type = BearingType.DeepGrooveBallBearing,
//            Category = BearingCategory.Standard,
//            Brand = "SKF",
//            InnerDiameter = 20,
//            OuterDiameter = 47,
//            Width = 14,
//            Material = "铬钢",
//            SealType = "双面接触式密封",
//            Status = BearingStatus.Active,
//            IsVerified = true,
//            VerificationLevel = VerificationLevel.Standard,
//            CreatedAt = DateTime.UtcNow,
//            UpdatedAt = DateTime.UtcNow
//        }
//    };

//    context.Bearings.AddRange(sampleBearings);
//    await context.SaveChangesAsync();
//}

// 示例数据种子
async Task SeedSampleDataAsync(BearingDbContext context)
{
    if (await context.Bearings.AnyAsync()) return;

    var sampleBearings = new List<Bearing>
    {
        new()
        {
            BearingNumber = "6204-2RS",
            DisplayName = "6204-2RS 深沟球轴承",
            Type = BearingType.DeepGrooveBallBearing,
            Category = BearingCategory.Standard,
            Brand = "SKF",
            InnerDiameter = 20,
            OuterDiameter = 47,
            Width = 14,
            Material = "铬钢",
            SealType = "双面接触式密封",
            Status = BearingStatus.Active,
            IsVerified = true,
            VerificationLevel = VerificationLevel.Standard,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        },
        new()
        {
            BearingNumber = "6305",
            DisplayName = "6305 深沟球轴承",
            Type = BearingType.DeepGrooveBallBearing,
            Category = BearingCategory.Standard,
            Brand = "NSK",
            InnerDiameter = 25,
            OuterDiameter = 62,
            Width = 17,
            Material = "铬钢",
            Status = BearingStatus.Active,
            IsVerified = true,
            VerificationLevel = VerificationLevel.Standard,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        },
        new()
        {
            BearingNumber = "32008",
            DisplayName = "32008 圆锥滚子轴承",
            Type = BearingType.TaperedRollerBearing,
            Category = BearingCategory.HeavyDuty,
            Brand = "TIMKEN",
            InnerDiameter = 40,
            OuterDiameter = 68,
            Width = 19,
            Material = "渗碳钢",
            Status = BearingStatus.Active,
            IsVerified = true,
            VerificationLevel = VerificationLevel.Standard,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedAt = DateTime.UtcNow.AddDays(-2)
        }
    };

    context.Bearings.AddRange(sampleBearings);
    await context.SaveChangesAsync();
}
