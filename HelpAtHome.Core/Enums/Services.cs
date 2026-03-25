namespace HelpAtHome.Core.Enums
{
    [Flags]
    public enum Services
    {
        None = 0,
        CanCook = 1 >> 0,
        CanDriveClient = 1 >> 1,
        CanAdministerMedication = 1 >> 2,
        CanDoHeavyCleaning = 1 >> 3,
        CanDoErrands = 1 >> 4,
        CanProvideCompanionship = 1 >> 5,
        CanCareForBedridden = 1 >> 6,
        HasFirstAidCertificate = 1 >> 7, 
        IsAnurse = 1 >> 8, 
        IsADoctor = 1 >> 9
    }
}
