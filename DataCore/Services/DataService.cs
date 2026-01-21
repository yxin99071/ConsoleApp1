using DataCore.Data;
using DataCore.Models;
using Microsoft.EntityFrameworkCore;


namespace DataCore.Services
{
    public class DataService
    {
        private readonly BattleDbContext _context;
        public DataService()
        {
            _context = DatabaseService.GetDbContext();
        }
        public async Task<User?> GetUserById(int UserId)
        {
            return await _context.Users
                .AsNoTracking()
                .Include(u => u.Weapons)
                .ThenInclude(w => w.WeaponBuffs)
                .ThenInclude(wb => wb.Buff)
                .Include(u => u.Skills)
                .ThenInclude(s => s.SkillBuffs)
                .ThenInclude(sb => sb.Buff)
                .SingleOrDefaultAsync(u => u.Id == UserId);
        }
        public async Task<List<Buff>> GetAllBuffs()
        {
            return await _context.Buffs.AsNoTracking().ToListAsync();
        }
        public async Task UpdateSinlgeUser(User user)
        {
            // 从本地缓存找，或者直接创建一个只带 ID 的占位对象
            var trackedUser = _context.Users.Local.FirstOrDefault(u => u.Id == user.Id);

            if (trackedUser == null)
            {
                // 如果没在跟踪，创建一个新容器来承载更新
                trackedUser = new User { Id = user.Id };
                _context.Users.Attach(trackedUser);
            }

            // 这一行会自动对比属性，只更新匹配的字段
            // 如果不想更新全部字段，可以创建一个只包含那几个属性的匿名对象
            _context.Entry(trackedUser).CurrentValues.SetValues(new
            {
                user.Exp,
                user.Level,
                user.Health,
                user.Agility,
                user.Strength,
                user.Intelligence
            });

            await _context.SaveChangesAsync();
        }
    }
}
