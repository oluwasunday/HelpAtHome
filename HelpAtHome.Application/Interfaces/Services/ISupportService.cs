using HelpAtHome.Core.DTOs.Requests;
using HelpAtHome.Core.DTOs.Responses;
using HelpAtHome.Core.Enums;
using HelpAtHome.Shared;

namespace HelpAtHome.Application.Interfaces.Services
{
    public interface ISupportService
    {
        Task<Result<TicketDto>> CreateTicketAsync(Guid userId, CreateTicketDto dto);
        Task<Result<TicketDetailDto>> GetTicketAsync(Guid userId, Guid ticketId, bool isAdmin = false);
        Task<Result<PagedResult<TicketDto>>> GetMyTicketsAsync(Guid userId, int page, int size);
        Task<Result<PagedResult<TicketDto>>> GetAllTicketsAsync(TicketStatus? status, TicketPriority? priority, int page, int size);
        Task<Result<TicketMessageDto>> AddMessageAsync(Guid userId, Guid ticketId, AddTicketMessageDto dto);
        Task<Result> AssignTicketAsync(Guid adminId, Guid ticketId, AssignTicketDto dto);
        Task<Result> UpdateStatusAsync(Guid adminId, Guid ticketId, UpdateTicketStatusDto dto);
        Task<Result> ResolveTicketAsync(Guid adminId, Guid ticketId, ResolveTicketDto dto);
    }
}
