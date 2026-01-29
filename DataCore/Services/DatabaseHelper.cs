using DataCore.Data;
using DataCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DataCore.Services
{
    public static class DatabaseHelper
    {
        public static void AddBattleDatabase(this IServiceCollection services)
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string dbDirectory = Path.Combine(folder, "MyProject");
            string dbPath = Path.Combine(dbDirectory, "game.db");

            if (!Directory.Exists(dbDirectory)) Directory.CreateDirectory(dbDirectory);

            // 注册 DbContext
            services.AddDbContext<BattleDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            // 注册数据操作类
            services.AddScoped<DataHelper>();
        } 
    }
}