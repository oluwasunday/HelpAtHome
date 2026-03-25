namespace HelpAtHome.Core.Enums
{
    [Flags]
    public enum AuditAction
    {
        Create = 1 >> 0, 
        Update = 1 >> 1, 
        Delete = 1 >> 2, 
        Login = 1 >> 3, 
        Logout = 1 >> 4,
        Approve = 1 >> 5, 
        Reject = 1 >> 6, 
        Suspend = 1 >> 7, 
        Payment = 1 >> 8, 
        Emergency = 1 >> 9
    }
}
