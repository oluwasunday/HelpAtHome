using AutoMapper;
using HelpAtHome.Application.Interfaces;
using HelpAtHome.Application.Interfaces.Services;
using HelpAtHome.Core.DTOs.Requests;
using HelpAtHome.Core.DTOs.Responses;
using HelpAtHome.Core.Entities;
using HelpAtHome.Core.Enums;
using HelpAtHome.Shared;

namespace HelpAtHome.Application.Services
{
    public class SupportService : ISupportService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly INotificationService _notifications;

        public SupportService(IUnitOfWork uow, IMapper mapper, INotificationService notifications)
        {
            _uow = uow;
            _mapper = mapper;
            _notifications = notifications;
        }

        public async Task<Result<TicketDto>> CreateTicketAsync(Guid userId, CreateTicketDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Subject))
                return Result<TicketDto>.Fail("Subject is required.");

            if (string.IsNullOrWhiteSpace(dto.Description))
                return Result<TicketDto>.Fail("Description is required.");

            if (dto.BookingId.HasValue)
            {
                var booking = await _uow.Bookings.GetByIdAsync(dto.BookingId.Value);
                if (booking == null)
                    return Result<TicketDto>.Fail("Booking not found.");
            }

            var ticketNumber = $"TKT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

            var ticket = new SupportTicket
            {
                Id = Guid.NewGuid(),
                TicketNumber = ticketNumber,
                RaisedByUserId = userId,
                BookingId = dto.BookingId,
                Subject = dto.Subject.Trim(),
                Description = dto.Description.Trim(),
                IsDispute = dto.IsDispute,
                Priority = dto.Priority,
                Status = TicketStatus.Open
            };

            await _uow.SupportTickets.AddAsync(ticket);
            await _uow.SaveChangesAsync();

            var saved = await _uow.SupportTickets.GetWithDetailsAsync(ticket.Id);
            return Result<TicketDto>.Ok(_mapper.Map<TicketDto>(saved));
        }

        public async Task<Result<TicketDetailDto>> GetTicketAsync(Guid userId, Guid ticketId, bool isAdmin = false)
        {
            var ticket = await _uow.SupportTickets.GetWithDetailsAsync(ticketId);
            if (ticket == null)
                return Result<TicketDetailDto>.Fail("Ticket not found.");

            if (!isAdmin && ticket.RaisedByUserId != userId)
                return Result<TicketDetailDto>.Fail("Access denied.");

            // Filter internal messages for non-admins
            if (!isAdmin && ticket.Messages != null)
                ticket.Messages = ticket.Messages.Where(m => !m.IsInternal).ToList();

            return Result<TicketDetailDto>.Ok(_mapper.Map<TicketDetailDto>(ticket));
        }

        public async Task<Result<PagedResult<TicketDto>>> GetMyTicketsAsync(Guid userId, int page, int size)
        {
            var (items, total) = await _uow.SupportTickets.GetPagedAsync(userId, null, null, page, size);
            var dtos = _mapper.Map<List<TicketDto>>(items);
            return Result<PagedResult<TicketDto>>.Ok(new PagedResult<TicketDto>(dtos, total, page, size));
        }

        public async Task<Result<PagedResult<TicketDto>>> GetAllTicketsAsync(
            TicketStatus? status, TicketPriority? priority, int page, int size)
        {
            var (items, total) = await _uow.SupportTickets.GetPagedAsync(null, status, priority, page, size);
            var dtos = _mapper.Map<List<TicketDto>>(items);
            return Result<PagedResult<TicketDto>>.Ok(new PagedResult<TicketDto>(dtos, total, page, size));
        }

        public async Task<Result<TicketMessageDto>> AddMessageAsync(Guid userId, Guid ticketId, AddTicketMessageDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Message))
                return Result<TicketMessageDto>.Fail("Message cannot be empty.");

            var ticket = await _uow.SupportTickets.FirstOrDefaultAsync(t => t.Id == ticketId);
            if (ticket == null)
                return Result<TicketMessageDto>.Fail("Ticket not found.");

            if (ticket.Status == TicketStatus.Closed)
                return Result<TicketMessageDto>.Fail("Cannot add messages to a closed ticket.");

            var user = await _uow.Users.GetByIdWithProfileAsync(userId);
            if (user == null)
                return Result<TicketMessageDto>.Fail("User not found.");

            bool isAdmin = user.Role == UserRole.Admin || user.Role == UserRole.SuperAdmin;
            bool isTicketOwner = ticket.RaisedByUserId == userId;

            if (!isAdmin && !isTicketOwner)
                return Result<TicketMessageDto>.Fail("Access denied.");

            if (dto.IsInternal && !isAdmin)
                return Result<TicketMessageDto>.Fail("Only admins can post internal notes.");

            var message = new TicketMessage
            {
                Id = Guid.NewGuid(),
                TicketId = ticketId,
                SenderUserId = userId,
                Message = dto.Message.Trim(),
                IsInternal = dto.IsInternal && isAdmin,
                AttachmentUrl = null
            };

            // Reopen if client replies to resolved ticket
            if (isTicketOwner && ticket.Status == TicketStatus.Resolved)
            {
                ticket.Status = TicketStatus.Open;
                ticket.ResolvedAt = null;
                _uow.SupportTickets.Update(ticket);
            }

            // Move to InProgress when admin replies to open ticket
            if (isAdmin && ticket.Status == TicketStatus.Open)
            {
                ticket.Status = TicketStatus.InProgress;
                _uow.SupportTickets.Update(ticket);
            }

            await _uow.TicketMessages.AddAsync(message);
            await _uow.SaveChangesAsync();

            // Notify the other party
            if (isAdmin && !message.IsInternal)
            {
                await _notifications.SendAsync(
                    ticket.RaisedByUserId,
                    "Support ticket updated",
                    $"An admin has replied to your ticket: {ticket.Subject}",
                    "TicketMessage", ticket.Id.ToString());
            }
            else if (!isAdmin)
            {
                // Notify assigned admin if any, or skip
                if (ticket.AssignedToAdminId.HasValue)
                {
                    await _notifications.SendAsync(
                        ticket.AssignedToAdminId.Value,
                        "New ticket reply",
                        $"User replied on ticket: {ticket.Subject}",
                        "TicketMessage", ticket.Id.ToString());
                }
            }

            // Reload with sender navigation
            var saved = await _uow.TicketMessages.FirstOrDefaultAsync(m => m.Id == message.Id);
            saved!.Sender = user;
            return Result<TicketMessageDto>.Ok(_mapper.Map<TicketMessageDto>(saved));
        }

        public async Task<Result> AssignTicketAsync(Guid adminId, Guid ticketId, AssignTicketDto dto)
        {
            var ticket = await _uow.SupportTickets.FirstOrDefaultAsync(t => t.Id == ticketId);
            if (ticket == null)
                return Result.Fail("Ticket not found.");

            var targetAdmin = await _uow.Users.GetByIdWithProfileAsync(dto.AdminUserId);
            if (targetAdmin == null || (targetAdmin.Role != UserRole.Admin && targetAdmin.Role != UserRole.SuperAdmin))
                return Result.Fail("Target user is not an admin.");

            ticket.AssignedToAdminId = dto.AdminUserId;
            if (ticket.Status == TicketStatus.Open)
                ticket.Status = TicketStatus.InProgress;

            _uow.SupportTickets.Update(ticket);
            await _uow.SaveChangesAsync();

            await _notifications.SendAsync(
                dto.AdminUserId,
                "Ticket assigned to you",
                $"Ticket #{ticket.TicketNumber} has been assigned to you.",
                "TicketAssigned", ticket.Id.ToString());

            return Result.Ok();
        }

        public async Task<Result> UpdateStatusAsync(Guid adminId, Guid ticketId, UpdateTicketStatusDto dto)
        {
            var ticket = await _uow.SupportTickets.FirstOrDefaultAsync(t => t.Id == ticketId);
            if (ticket == null)
                return Result.Fail("Ticket not found.");

            ticket.Status = dto.Status;
            if (dto.Status == TicketStatus.Resolved)
                ticket.ResolvedAt = DateTime.UtcNow;

            _uow.SupportTickets.Update(ticket);
            await _uow.SaveChangesAsync();
            return Result.Ok();
        }

        public async Task<Result> ResolveTicketAsync(Guid adminId, Guid ticketId, ResolveTicketDto dto)
        {
            var ticket = await _uow.SupportTickets.FirstOrDefaultAsync(t => t.Id == ticketId);
            if (ticket == null)
                return Result.Fail("Ticket not found.");

            if (ticket.Status == TicketStatus.Closed)
                return Result.Fail("Ticket is already closed.");

            ticket.Status = TicketStatus.Resolved;
            ticket.ResolvedAt = DateTime.UtcNow;
            ticket.ResolutionNote = dto.ResolutionNote?.Trim();
            _uow.SupportTickets.Update(ticket);
            await _uow.SaveChangesAsync();

            await _notifications.SendAsync(
                ticket.RaisedByUserId,
                "Support ticket resolved",
                $"Your ticket #{ticket.TicketNumber} has been resolved.",
                "TicketResolved", ticket.Id.ToString());

            return Result.Ok();
        }
    }
}
