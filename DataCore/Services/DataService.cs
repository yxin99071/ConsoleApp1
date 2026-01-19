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

    }
}
