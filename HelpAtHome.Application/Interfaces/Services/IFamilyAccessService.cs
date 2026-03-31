using HelpAtHome.Core.DTOs.Requests;
using HelpAtHome.Core.DTOs.Responses;
using HelpAtHome.Shared;

namespace HelpAtHome.Application.Interfaces.Services
{
    public interface IFamilyAccessService
    {
        Task<Result<FamilyAccessDto>> InviteFamilyMemberAsync(Guid clientUserId, InviteFamilyMemberDto dto);
        Task<Result> ApproveAccessAsync(Guid familyMemberUserId, Guid accessId);
        Task<Result> RevokeAccessAsync(Guid clientUserId, Guid accessId);
        Task<Result<FamilyAccessDto>> UpdateAccessAsync(Guid clientUserId, Guid accessId, UpdateFamilyAccessDto dto);
        Task<Result<List<FamilyAccessDto>>> GetClientFamilyAccessesAsync(Guid clientUserId);
        Task<Result<List<FamilyAccessDto>>> GetMyAccessesAsync(Guid familyMemberUserId);
        Task<Result<FamilyClientViewDto>> GetClientViewAsync(Guid familyMemberUserId, Guid clientUserId);
    }
}
