using HHPortal.Backend.Data;
using HHPortal.Backend.Models;
using HHPortal.Backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using System.Text;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// 添加数据库上下文
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 配置JWT设置
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

// 注册JWT服务
builder.Services.AddSingleton<IJwtTokenService>(provider =>
{
    var jwtSettings = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<JwtSettings>>().Value;
    return new JwtTokenService(jwtSettings);
});

// 注册认证服务
builder.Services.AddScoped<IAuthService, AuthService>();

// 添加认证
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings?.Issuer,
        ValidAudience = jwtSettings?.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.SecretKey ?? string.Empty))
    };
});

// 添加Identity服务
builder.Services.AddIdentity<User, Role>(options => { })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// 添加授权
builder.Services.AddAuthorization();

// 添加控制器
builder.Services.AddControllers();

// 添加Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 添加CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.WebHost.UseUrls("http://localhost:5257");
var app = builder.Build();

// 验证静态文件路径配置
var staticFilesPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "../html"));
var staticFileLogger = app.Services.GetRequiredService<ILogger<Program>>();
staticFileLogger.LogInformation("ContentRootPath: {ContentRootPath}", builder.Environment.ContentRootPath);
staticFileLogger.LogInformation("静态文件路径: {StaticFilesPath}", staticFilesPath);
var indexHtmlPath = Path.Combine(staticFilesPath, "index.html");
staticFileLogger.LogInformation("index.html是否存在: {Exists}", File.Exists(indexHtmlPath));

// 配置HTTP请求管道
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "../html"))
    )
});
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");

// 初始化数据库
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await SeedData.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "初始化数据库时发生错误");
    }
}

app.Run();
