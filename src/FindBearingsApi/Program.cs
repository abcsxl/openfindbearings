using FindBearingsApi.Application.Services;
using FindBearingsApi.Infrastructure.Persistence;
using FindBearingsApi.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// ====== JWT 配置 ======
//// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddEndpointsApiExplorer(); // 启用 API 探索器（必须）
//builder.Services.AddOpenApi(); // 使用内置 OpenAPI 支持
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FindBearings API",
        Version = "v1"
    });

    // 添加 JWT 授权
    // 定义安全方案
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });
    // 应用安全要求
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

// ====== JWT 配置 ======
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// ====== 数据库上下文 ====== 
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// ====== 服务注册 ====== 
builder.Services.AddScoped<IRecommendationService, RecommendationService>();
builder.Services.AddScoped<IWeChatNotificationService, WeChatNotificationService>();

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMiniProgram", policy =>
    {
        policy.WithOrigins("https://abcsxl.com") // 替换为你的小程序域名
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    //app.MapOpenApi(); // 提供 /openapi/v1/swagger.json
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FindBearings API v1");
        c.RoutePrefix = "swagger"; // 访问 /swagger
        c.ConfigObject.AdditionalItems["persistAuthorization"] = true;
    });
}

app.UseCors("AllowMiniProgram");
app.UseHttpsRedirection();

app.UseAuthentication(); // 必须在 UseAuthorization 之前！
app.UseAuthorization();

app.MapControllers();

app.Run();