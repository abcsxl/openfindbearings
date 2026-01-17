using FindBearingsApi.Application.Services;
using FindBearingsApi.Endpoints;
using FindBearingsApi.Infrastructure.Persistence;
using FindBearingsApi.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// ====== Swagger 配置 ======
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

// 配置选项
builder.Services.Configure<WeChatTokenOptions>(builder.Configuration.GetSection("WeChat"));
builder.Services.Configure<WeChatNotificationOptions>(builder.Configuration.GetSection("WeChatNotifications"));

builder.Services.AddAuthorization();

// ====== 服务注册 ====== 
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IWeChatTokenService, WeChatTokenService>();
builder.Services.AddScoped<IWeChatNotificationService, WeChatNotificationService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IMyMessageService, MyMessageService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();

builder.Services.AddHttpClient<WeChatNotificationService>();

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

// 注册端点
app.MapMessageEndpoints();
app.MapAuthEndpoints();
app.MapNotificationEndpoints();
app.MapMyMessageEndpoints();

app.Run();