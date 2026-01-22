
using BattleBackend.Controllers;

namespace BattleBackend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            //依赖注入
            builder.Services.AddTransient<BattleController>();
            builder.Services.AddScoped<JwtService>();
            // 1. 定义 CORS 策略名
            const string MyAllowVueApp = "_myAllowVueApp";

            // 2. 配置服务
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowVueApp,
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:5173") // 匹配你的 Vite 端口
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowCredentials();
                    });
            });


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // 必须在 UseRouting 之后，UseAuthorization 之前
            app.UseCors(MyAllowVueApp);
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
