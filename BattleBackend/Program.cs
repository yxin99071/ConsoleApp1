using BattleBackend.Services;
using DataCore.Models;
using DataCore.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace BattleBackend
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // --- 1. 注册认证服务 (必须配置如何验证 Token) ---
            var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new Exception("JWT Key 未配置");
            var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,   // 如果你 GenerateToken 时没写 Issuer，这里就选 false
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                    ClockSkew = TimeSpan.Zero // 立即过期，没有缓冲时间
                };
            });

            // --- 2. 基础服务注册 ---
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddBattleDatabase();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "API", Version = "v1" });

                // 🔐 定义 Bearer 认证
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "在这里输入：Bearer {你的Token}"
                });

                // 🔐 应用到所有接口
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
            });

            // 依赖注入 (注意：Controller 默认是自动注册的，除非你有特殊需求，否则不需要 AddTransient<BattleController>)
            builder.Services.AddScoped<JwtService>();
            builder.Services.AddScoped<BattleService>();

            // CORS 配置 (保持你的不变)
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("VueCorsPolicy", policy =>
                {
                    policy.WithOrigins("http://localhost:5173") // 替换为你前端 Vite 的实际运行地址
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials(); // 必须！对应前端的 withCredentials: true
                });
            });



            var app = builder.Build();

            // --- 3. 中间件管道顺序 (核心！) ---
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseRouting(); // 建议显式加上，确保顺序清晰

            app.UseCors("VueCorsPolicy");

            app.UseAuthentication(); // 必须在 Authorization 之前！识别你是谁
            app.UseAuthorization();  // 检查你有没有权限

            app.MapControllers();

            using (var scope = app.Services.CreateScope())
            {
                var dataHelper = scope.ServiceProvider.GetRequiredService<DataHelper>();
                await dataHelper.SeedData();
            }

            app.Run();
        }
    }
}
