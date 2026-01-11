using Microsoft.EntityFrameworkCore;
using SupplierApi.Data;
using SupplierApi.Services;

var builder = WebApplication.CreateBuilder(args);

// 添加Dapr
builder.Services.AddDaprClient();

// 添加数据库上下文
builder.Services.AddDbContext<SupplierDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// 添加服务
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();

// 添加控制器
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthorization();

app.MapControllers();

// 初始化数据库
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SupplierDbContext>();
    await context.Database.EnsureCreatedAsync();
}

app.Run();