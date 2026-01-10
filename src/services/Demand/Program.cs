using Demand.Data;
using Demand.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 添加配置
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// 添加Dapr
builder.Services.AddDaprClient();

// 添加数据库上下文
builder.Services.AddDbContext<DemandDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// 添加服务
builder.Services.AddScoped<IDemandService, DemandService>();
builder.Services.AddScoped<IDemandRepository, DemandRepository>();
builder.Services.AddScoped<IDemandMatchingService, DemandMatchingService>();

// 添加控制器
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 配置中间件
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

// 初始化数据库
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DemandDbContext>();
    await context.Database.EnsureCreatedAsync();
}

app.Run();