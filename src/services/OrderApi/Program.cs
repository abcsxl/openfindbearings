using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using OrderApi.Data;
using OrderApi.Services;

var builder = WebApplication.CreateBuilder(args);


// 添加配置
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// 添加服务到容器
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// CORS 配置
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        policy.WithOrigins(allowedOrigins ?? new[] { "http://localhost:3000" })
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Dapr 配置
builder.Services.AddDaprClient(builder =>
{
    builder.UseHttpEndpoint($"http://localhost:3604");
    builder.UseGrpcEndpoint($"http://localhost:60004");
});

// 数据库上下文
builder.Services.AddDbContext<OrderDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("OrderDb");
    if (string.IsNullOrEmpty(connectionString))
    {
        options.UseInMemoryDatabase("OrderDb");
        Console.WriteLine("使用内存数据库");
    }
    else
    {
        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null);
            npgsqlOptions.CommandTimeout(60);
        });
        Console.WriteLine($"使用 PostgreSQL: {connectionString.Split(';')[0]}");
    }
});

// 注册服务
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddSingleton<OrderHealthCheck>();

// Swagger 配置
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Order API",
        Version = "v1",
        Description = "轴承订单管理API",
        Contact = new OpenApiContact
        {
            Name = "API Support",
            Email = "support@openfindbearings.com"
        }
    });

    c.EnableAnnotations();
});

// 健康检查
builder.Services.AddHealthChecks()
    .AddDbContextCheck<OrderDbContext>()
    .AddCheck<OrderHealthCheck>("order_health");

// 日志配置
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
});

var app = builder.Build();

// 配置 HTTP 请求管道
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order API v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "Order API Documentation";
    });

    app.UseCors("AllowSpecificOrigins");
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// 数据库迁移和种子数据
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<OrderDbContext>();

        if (context.Database.IsRelational())
        {
            await context.Database.MigrateAsync();
            Console.WriteLine("数据库迁移完成");
        }

        if (app.Environment.IsDevelopment())
        {
            await SeedData.InitializeAsync(context);
            Console.WriteLine("种子数据初始化完成");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "数据库初始化失败");
    }
}

app.Logger.LogInformation("Order API 启动成功");
app.Logger.LogInformation("环境: {Environment}", app.Environment.EnvironmentName);
app.Logger.LogInformation("地址: http://localhost:5004");
app.Logger.LogInformation("Dapr HTTP端口: 3604, gRPC端口: 60004");

await app.RunAsync();