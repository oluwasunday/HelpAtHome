using HelpAtHome.Core.Entities;
using HelpAtHome.Core.Enums;

namespace HelpAtHome.Application.Interfaces.Repositories
{
    // IUserRepository handles only custom queries not available on UserManager.
    public interface IUserRepository
    {
        Task<User?> GetByPhoneAsync(string phone);
        Task<User?> GetByIdWithProfileAsync(Guid id);
        Task<IEnumerable<User>> GetByRoleAsync(UserRole role);
        Task<bool> PhoneExistsAsync(string phoneNumber);
        //Task<PagedResult<User>> GetPagedUsersAsync(UserRole? role, bool? isSuspended, int page, int size);
    }


}
