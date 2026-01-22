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
        public async Task UpgradeSinlgeUser(User user)
        {
            var dbUser = await _context.Users.FindAsync(user.Id);

            if (dbUser != null)
            {
                // 核心：强制从数据库同步最新状态，刷新内存快照
                await _context.Entry(dbUser).ReloadAsync();

                _context.Entry(dbUser).CurrentValues.SetValues(new
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
}
