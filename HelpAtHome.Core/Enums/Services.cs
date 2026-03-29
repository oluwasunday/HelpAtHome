namespace HelpAtHome.Core.Enums
{
    [Flags]
    public enum Services
    {
        None = 0,
        CanCook = 1 << 0,                 // 1
        CanDriveClient = 1 << 1,          // 2
        CanAdministerMedication = 1 << 2, // 4
        CanDoHeavyCleaning = 1 << 3,      // 8
        CanDoErrands = 1 << 4,            // 16
        CanProvideCompanionship = 1 << 5, // 32
        CanCareForBedridden = 1 << 6,     // 64
        HasFirstAidCertificate = 1 << 7,  // 128
        IsANurse = 1 << 8,                // 256
        IsADoctor = 1 << 9                // 512
    }
}
