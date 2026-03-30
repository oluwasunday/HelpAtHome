using HelpAtHome.Application.Interfaces.Repositories;
using HelpAtHome.Core.Entities;
using HelpAtHome.Core.Enums;
using HelpAtHome.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpAtHome.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _ctx;

        public UserRepository(AppDbContext context) => _ctx = context;

        public async Task<User?> GetByPhoneAsync(string phone)
            => await _ctx.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone && !u.IsDeleted);

        public async Task<bool> PhoneExistsAsync(string phoneNumber)
            => await _ctx.Users.AnyAsync(u => u.PhoneNumber == phoneNumber && !u.IsDeleted);

        public async Task<User?> GetByIdWithProfileAsync(Guid id)
            => await _ctx.Users
                .Include(u => u.CaregiverProfile).ThenInclude(c => c!.Address)
                .Include(u => u.CaregiverProfile).ThenInclude(c => c!.CaregiverServices)
                    .ThenInclude(cs => cs.ServiceCategory)
                .Include(u => u.ClientProfile)
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

        public async Task<IEnumerable<User>> GetByRoleAsync(UserRole role)
            => await _ctx.Users
                .Where(u => u.Role == role && !u.IsDeleted)
                .ToListAsync();
    }
}
