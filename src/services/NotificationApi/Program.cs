using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi;
using NotificationApi.Data;
using NotificationApi.Models.Configuration;
using NotificationApi.Services;

var builder = WebApplication.CreateBuilder(args);

// 添加配置
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// 添加服务
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Notification API",
        Version = "v1",
        Description = "轴承采购平台通知服务API",
        Contact = new OpenApiContact
        {
            Name = "API Support",
            Email = "support@openfindbearings.com"
        }
    });
});

// 数据库
builder.Services.AddDbContext<NotificationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("NotificationDb");
    if (string.IsNullOrEmpty(connectionString))
    {
        // 开发环境使用内存数据库
        options.UseInMemoryDatabase("NotificationDb");
        Console.WriteLine("使用内存数据库进行开发");
    }
    else
    {
        options.UseNpgsql(connectionString);
        Console.WriteLine($"使用数据库连接");
    }
});

// 配置
builder.Services.Configure<NotificationConfig>(
    builder.Configuration.GetSection("NotificationConfig"));

// 依赖注入
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<ISmsSender, SmsSender>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Dapr
builder.Services.AddDaprClient();
builder.Services.AddControllers().AddDapr();

// 健康检查（简化版）
builder.Services.AddHealthChecks()
    .AddCheck("api_health_check", () => HealthCheckResult.Healthy("API is healthy"));

var app = builder.Build();

// 配置HTTP管道
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Notification API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseRouting();
app.UseAuthorization();

// 健康检查端点
app.MapHealthChecks("/health");

// 首页
app.MapGet("/", () => "Notification API is running");

// 控制器
app.MapControllers();

// Dapr
app.UseCloudEvents();
app.MapSubscribeHandler();

// 初始化数据库
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    if (context.Database.IsRelational())
    {
        await context.Database.MigrateAsync();
    }
    await SeedData.InitializeAsync(context);
}

app.Logger.LogInformation("Notification API 启动成功");
app.Logger.LogInformation("环境: {Environment}", app.Environment.EnvironmentName);
app.Logger.LogInformation("地址: http://localhost:5005");

await app.RunAsync();