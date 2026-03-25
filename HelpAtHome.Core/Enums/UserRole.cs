namespace HelpAtHome.Core.Enums
{
    public enum UserRole
    {
        SuperAdmin = 1,
        Admin = 2,
        Client = 3,
        IndividualCaregiver = 4,
        AgencyAdmin = 5,
        AgencyCaregiver = 6,
        FamilyMember = 7
    }

    public enum PaymentStatus 
    { 
        Pending,
        Paid, 
        Failed, 
        Refunded 
    }
    public enum TransactionType 
    { 
        Deposit, 
        Withdrawal, 
        Booking, 
        Refund, 
        Commission, 
        Payout 
    }
    public enum TransactionStatus 
    { 
        Pending, 
        Success, 
        Failed 
    }
    public enum DocumentType
    {
        NationalId = 1, 
        DriverLicense, 
        Passport,
        PoliceReport, 
        Certificate, 
        AgencyRegistration
    }
    public enum VerificationStatus { Pending, Approved, Rejected }
    public enum TicketStatus { Closed, Open, InProgress, Resolved }
    public enum TicketPriority { Low, Medium, High, Urgent }
    public enum AlertStatus { FalseAlarm, Active, Responded, Resolved }
    public enum AccessLevel { ViewOnly = 1, Notify, FullAccess }
    public enum FrequencyType { OneTime, Daily, Weekly, Monthly }
}
