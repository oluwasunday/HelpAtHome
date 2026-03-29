using AutoMapper;
using HelpAtHome.Core.DTOs.Common;
using HelpAtHome.Core.DTOs.Requests;
using HelpAtHome.Core.DTOs.Responses;
using HelpAtHome.Core.Entities;

namespace HelpAtHome.Application
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ── User ───────────────────────────────────────────────────
            CreateMap<User, UserSummaryDto>()
                .ForMember(d => d.FullName, o => o.MapFrom(s => s.FirstName + " " + s.LastName))
                .ForMember(d => d.Role, o => o.MapFrom(s => s.Role.ToString()));

            CreateMap<User, AdminUserDto>()
                .ForMember(d => d.FullName, o => o.MapFrom(s => s.FirstName + " " + s.LastName))
                .ForMember(d => d.Role, o => o.MapFrom(s => s.Role.ToString()));

            // ── CaregiverProfile ──────────────────────────────────────
            CreateMap<CaregiverProfile, CaregiverSummaryDto>()
                .ForMember(d => d.Badge, o => o.MapFrom(s => s.Badge.ToString()))
                .ForMember(d => d.GenderProvided, o => o.MapFrom(s => s.GenderProvided.ToString()))
                .ForMember(d => d.VerificationStatus, o => o.MapFrom(s => s.VerificationStatus.ToString()))
                .ForMember(d => d.Services, o => o.MapFrom(s =>
                    s.CaregiverServices.Select(cs => cs.ServiceCategory.Name).ToList()))
                .ForMember(d => d.AgencyName, o => o.MapFrom(s => s.Agency != null ? s.Agency.AgencyName : null));

            CreateMap<CaregiverProfile, CaregiverProfileDto>();

            // ── ClientProfile ─────────────────────────────────────────
            CreateMap<ClientProfile, ClientProfileDto>()
                .ForMember(d => d.CaregiverGenderPreference,
                    o => o.MapFrom(s => s.Gender.ToString()));

            // ── Agency ───────────────────────────────────────────────
            CreateMap<Agency, AgencySummaryDto>()
                .ForMember(d => d.VerificationStatus,
                    o => o.MapFrom(s => s.VerificationStatus.ToString()));
            CreateMap<Agency, AgencyDto>();

            // ── Booking ───────────────────────────────────────────────
            CreateMap<Booking, BookingDto>()
                .ForMember(d => d.Status,      o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.Frequency,   o => o.MapFrom(s => s.Frequency.ToString()))
                .ForMember(d => d.PaymentStatus, o => o.MapFrom(s => s.PaymentStatus.ToString()))
                .ForMember(d => d.ServiceCategory,
                    o => o.MapFrom(s => s.ServiceCategory != null ? s.ServiceCategory.Name : string.Empty))
                .ForMember(d => d.Caregiver, o => o.MapFrom(s => s.CaregiverProfile))
                .ForMember(d => d.Client,    o => o.MapFrom(s => s.ClientProfile != null ? s.ClientProfile.User : null));
            CreateMap<Booking, BookingDetailDto>();

            // ── Wallet / Transaction ──────────────────────────────────
            CreateMap<Wallet, WalletDto>();
            CreateMap<Transaction, TransactionDto>()
                .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()))
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.BookingReference,
                    o => o.MapFrom(s => s.Booking != null ? s.Booking.BookingReference : null));

            // ── Review ────────────────────────────────────────────────
            CreateMap<Review, ReviewDto>()
                .ForMember(d => d.BookingRef,
                    o => o.MapFrom(s => s.Booking != null ? s.Booking.BookingReference : string.Empty));

            // ── Notification ──────────────────────────────────────────
            CreateMap<Notification, NotificationDto>();

            // ── SupportTicket ─────────────────────────────────────────
            CreateMap<SupportTicket, TicketDto>()
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.Priority, o => o.MapFrom(s => s.Priority.ToString()))
                .ForMember(d => d.BookingRef,
                    o => o.MapFrom(s => s.Booking != null ? s.Booking.BookingReference : null));
            CreateMap<SupportTicket, TicketDetailDto>();
            CreateMap<TicketMessage, TicketMessageDto>();

            // ── EmergencyAlert ────────────────────────────────────────
            CreateMap<EmergencyAlert, EmergencyAlertDto>()
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

            // ── FamilyAccess ──────────────────────────────────────────
            CreateMap<FamilyAccess, FamilyAccessDto>()
                .ForMember(d => d.AccessLevel, o => o.MapFrom(s => s.AccessLevel.ToString()));

            // ── VerificationDocument ──────────────────────────────────
            CreateMap<VerificationDocument, VerificationDocumentDto>()
                .ForMember(d => d.DocumentType, o => o.MapFrom(s => s.DocumentType.ToString()))
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

            // ── ServiceCategory ───────────────────────────────────────
            CreateMap<ServiceCategory, ServiceCategoryDto>();

            // ── Address entity → AddressDto ──────────────────────────────────────
            CreateMap<ClientAddress, AddressDto>();
            CreateMap<AgencyAddress, AddressDto>();
            CreateMap<CaregiverAddress, AddressDto>();

            // ── AddressUpsertDto → Address entities ──────────────────────────────
            CreateMap<AddressUpsertDto, ClientAddress>().ReverseMap();
            CreateMap<AddressUpsertDto, AgencyAddress>().ReverseMap();
            CreateMap<AddressUpsertDto, CaregiverAddress>().ReverseMap();

            // ── Update ClientProfile mapping to include nested Address ────────────
            CreateMap<ClientProfile, ClientProfileDto>()
                .ForMember(d => d.Address,
                           o => o.MapFrom(s => s.Address));

            // ── Update CaregiverProfile mapping ──────────────────────────────────
            CreateMap<CaregiverProfile, CaregiverSummaryDto>()
                .ForMember(d => d.Address,
                           o => o.MapFrom(s => s.Address));

            CreateMap<CaregiverProfile, CaregiverProfileDto>()
                .IncludeBase<CaregiverProfile, CaregiverSummaryDto>();

            // ── Update Agency mapping ─────────────────────────────────────────────
            CreateMap<Agency, AgencySummaryDto>()
                .ForMember(d => d.Address, o => o.MapFrom(s => s.AgencyAddress));

            CreateMap<Agency, AgencyDto>()
                .IncludeBase<Agency, AgencySummaryDto>();

        }
    }

}
