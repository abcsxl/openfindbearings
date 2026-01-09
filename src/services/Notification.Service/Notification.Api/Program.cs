using Dapr.AspNetCore;
using MediatR;
using Microsoft.OpenApi.Models;
using OpenFindBearings.Notification.Core.Events;
using OpenFindBearings.Notification.Core.Notifications;
using OpenFindBearings.Shared.Domain;
using OpenFindBearings.Shared.Domain.Events;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// 添加 Dapr 客户端
builder.Services.AddDaprClient();

// 添加 MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<StockChangedEventHandler>());

// 注册通知服务
builder.Services.AddScoped<INotificationService, NotificationService>();

// 配置邮件选项
builder.Services.Configure<EmailOptions>(
    builder.Configuration.GetSection(EmailOptions.SectionName));

// 添加控制器
builder.Services.AddControllers();

// 添加 API 文档
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Notification Service API",
        Version = "v1",
        Description = "通知服务 - 基于 Dapr 的微服务"
    });
});

// 添加 CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// 配置 HTTP 管道
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Notification Service API v1");
    });
}

app.UseCors("AllowAll");
app.UseCloudEvents(); // Dapr CloudEvents 支持
app.MapControllers();
app.MapSubscribeHandler(); // Dapr 订阅端点

// 健康检查
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "notification-service" }))
   .WithName("HealthCheck");

app.MapGet("/", () => Results.Ok(new
{
    service = "Notification Service",
    version = "1.0.0",
    description = "基于 Dapr 的通知服务",
    endpoints = new[]
    {
        "/health",
        "/swagger",
        "/api/notifications"
    }
})).WithName("Root");

app.Run();
