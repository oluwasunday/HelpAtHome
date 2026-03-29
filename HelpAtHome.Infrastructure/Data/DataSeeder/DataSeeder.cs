using HelpAtHome.Core.Entities;
using HelpAtHome.Core.Enums;
using HelpAtHome.Core.MongoDocuments;
using HelpAtHome.Infrastructure.MongoDB;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace HelpAtHome.Infrastructure.Data.DataSeeder
{
    public static class DataSeeder
    {
    
        // ── User GUIDs ────────────────────────────────────────────────────
        public static readonly Guid SuperAdminId = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000001");
        public static readonly Guid Admin1Id = Guid.Parse("aaaaaaaa-0002-0002-0002-000000000002");
        public static readonly Guid Admin2Id = Guid.Parse("aaaaaaaa-0003-0003-0003-000000000003");
        public static readonly Guid AgencyAdmin1Id = Guid.Parse("aaaaaaaa-0004-0004-0004-000000000004");
        public static readonly Guid AgencyAdmin2Id = Guid.Parse("aaaaaaaa-0005-0005-0005-000000000005");
        public static readonly Guid Caregiver1Id = Guid.Parse("aaaaaaaa-0006-0006-0006-000000000006");
        public static readonly Guid Caregiver2Id = Guid.Parse("aaaaaaaa-0007-0007-0007-000000000007");
        public static readonly Guid Caregiver3Id = Guid.Parse("aaaaaaaa-0008-0008-0008-000000000008");
        public static readonly Guid Caregiver4Id = Guid.Parse("aaaaaaaa-0009-0009-0009-000000000009");
        public static readonly Guid Caregiver5Id = Guid.Parse("aaaaaaaa-0010-0010-0010-000000000010");
        public static readonly Guid Client1Id = Guid.Parse("aaaaaaaa-0011-0011-0011-000000000011");
        public static readonly Guid Client2Id = Guid.Parse("aaaaaaaa-0012-0012-0012-000000000012");
        public static readonly Guid Client3Id = Guid.Parse("aaaaaaaa-0013-0013-0013-000000000013");
        public static readonly Guid Client4Id = Guid.Parse("aaaaaaaa-0014-0014-0014-000000000014");
        public static readonly Guid Family1Id = Guid.Parse("aaaaaaaa-0015-0015-0015-000000000015");
        public static readonly Guid Family2Id = Guid.Parse("aaaaaaaa-0016-0016-0016-000000000016");

        // ── Agency GUIDs ──────────────────────────────────────────────────
        public static readonly Guid Agency1Id = Guid.Parse("bbbbbbbb-0001-0001-0001-000000000001");
        public static readonly Guid Agency2Id = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000002");

        // ── CaregiverProfile GUIDs ────────────────────────────────────────
        public static readonly Guid CgProfile1Id = Guid.Parse("cccccccc-0001-0001-0001-000000000001");
        public static readonly Guid CgProfile2Id = Guid.Parse("cccccccc-0002-0002-0002-000000000002");
        public static readonly Guid CgProfile3Id = Guid.Parse("cccccccc-0003-0003-0003-000000000003");
        public static readonly Guid CgProfile4Id = Guid.Parse("cccccccc-0004-0004-0004-000000000004");
        public static readonly Guid CgProfile5Id = Guid.Parse("cccccccc-0005-0005-0005-000000000005");

        // ── ClientProfile GUIDs ───────────────────────────────────────────
        public static readonly Guid ClientProfile1Id = Guid.Parse("dddddddd-0001-0001-0001-000000000001");
        public static readonly Guid ClientProfile2Id = Guid.Parse("dddddddd-0002-0002-0002-000000000002");
        public static readonly Guid ClientProfile3Id = Guid.Parse("dddddddd-0003-0003-0003-000000000003");
        public static readonly Guid ClientProfile4Id = Guid.Parse("dddddddd-0004-0004-0004-000000000004");

        // ── ServiceCategory GUIDs ─────────────────────────────────────────
        public static readonly Guid CatElderly = Guid.Parse("eeeeeeee-0001-0001-0001-000000000001");
        public static readonly Guid CatCooking = Guid.Parse("eeeeeeee-0002-0002-0002-000000000002");
        public static readonly Guid CatCleaning = Guid.Parse("eeeeeeee-0003-0003-0003-000000000003");
        public static readonly Guid CatErrands = Guid.Parse("eeeeeeee-0004-0004-0004-000000000004");
        public static readonly Guid CatMedication = Guid.Parse("eeeeeeee-0005-0005-0005-000000000005");
        public static readonly Guid CatCompanion = Guid.Parse("eeeeeeee-0006-0006-0006-000000000006");

        // ── Booking GUIDs ─────────────────────────────────────────────────
        public static readonly Guid Booking1Id = Guid.Parse("ffffffff-0001-0001-0001-000000000001");
        public static readonly Guid Booking2Id = Guid.Parse("ffffffff-0002-0002-0002-000000000002");
        public static readonly Guid Booking3Id = Guid.Parse("ffffffff-0003-0003-0003-000000000003");
        public static readonly Guid Booking4Id = Guid.Parse("ffffffff-0004-0004-0004-000000000004");
        public static readonly Guid Booking5Id = Guid.Parse("ffffffff-0005-0005-0005-000000000005");

        // ── Wallet GUIDs ──────────────────────────────────────────────────
        // One wallet per user (13 users + 2 agency admin wallets = 13 wallets covering all users)
        public static readonly Guid Wallet_SA = Guid.Parse("11111111-0001-0001-0001-000000000001");
        public static readonly Guid Wallet_A1 = Guid.Parse("11111111-0002-0002-0002-000000000002");
        public static readonly Guid Wallet_A2 = Guid.Parse("11111111-0003-0003-0003-000000000003");
        public static readonly Guid Wallet_AA1 = Guid.Parse("11111111-0004-0004-0004-000000000004");
        public static readonly Guid Wallet_AA2 = Guid.Parse("11111111-0005-0005-0005-000000000005");
        public static readonly Guid Wallet_CG1 = Guid.Parse("11111111-0006-0006-0006-000000000006");
        public static readonly Guid Wallet_CG2 = Guid.Parse("11111111-0007-0007-0007-000000000007");
        public static readonly Guid Wallet_CG3 = Guid.Parse("11111111-0008-0008-0008-000000000008");
        public static readonly Guid Wallet_CG4 = Guid.Parse("11111111-0009-0009-0009-000000000009");
        public static readonly Guid Wallet_CG5 = Guid.Parse("11111111-0010-0010-0010-000000000010");
        public static readonly Guid Wallet_CL1 = Guid.Parse("11111111-0011-0011-0011-000000000011");
        public static readonly Guid Wallet_CL2 = Guid.Parse("11111111-0012-0012-0012-000000000012");
        public static readonly Guid Wallet_CL3 = Guid.Parse("11111111-0013-0013-0013-000000000013");
        public static readonly Guid Wallet_CL4 = Guid.Parse("11111111-0014-0014-0014-000000000014");

        // ── Review GUIDs ──────────────────────────────────────────────────
        public static readonly Guid Review1Id = Guid.Parse("22222222-0001-0001-0001-000000000001");
        public static readonly Guid Review2Id = Guid.Parse("22222222-0002-0002-0002-000000000002");
        public static readonly Guid Review3Id = Guid.Parse("22222222-0003-0003-0003-000000000003");
        public static readonly Guid Review4Id = Guid.Parse("22222222-0004-0004-0004-000000000004");

        // ── SupportTicket GUIDs ───────────────────────────────────────────
        public static readonly Guid Ticket1Id = Guid.Parse("33333333-0001-0001-0001-000000000001");
        public static readonly Guid Ticket2Id = Guid.Parse("33333333-0002-0002-0002-000000000002");
        public static readonly Guid Ticket3Id = Guid.Parse("33333333-0003-0003-0003-000000000003");
        public static readonly Guid Ticket4Id = Guid.Parse("33333333-0004-0004-0004-000000000004");

        // ── EmergencyAlert GUIDs ──────────────────────────────────────────
        public static readonly Guid Alert1Id = Guid.Parse("44444444-0001-0001-0001-000000000001");
        public static readonly Guid Alert2Id = Guid.Parse("44444444-0002-0002-0002-000000000002");
        public static readonly Guid Alert3Id = Guid.Parse("44444444-0003-0003-0003-000000000003");
        public static readonly Guid Alert4Id = Guid.Parse("44444444-0004-0004-0004-000000000004");

        // ── FamilyAccess GUIDs ────────────────────────────────────────────
        public static readonly Guid FAccess1Id = Guid.Parse("55555555-0001-0001-0001-000000000001");
        public static readonly Guid FAccess2Id = Guid.Parse("55555555-0002-0002-0002-000000000002");
        public static readonly Guid FAccess3Id = Guid.Parse("55555555-0003-0003-0003-000000000003");
        public static readonly Guid FAccess4Id = Guid.Parse("55555555-0004-0004-0004-000000000004");

        // ── VerificationDocument GUIDs ────────────────────────────────────
        public static readonly Guid VDoc1Id = Guid.Parse("66666666-0001-0001-0001-000000000001");
        public static readonly Guid VDoc2Id = Guid.Parse("66666666-0002-0002-0002-000000000002");
        public static readonly Guid VDoc3Id = Guid.Parse("66666666-0003-0003-0003-000000000003");
        public static readonly Guid VDoc4Id = Guid.Parse("66666666-0004-0004-0004-000000000004");
        public static readonly Guid VDoc5Id = Guid.Parse("66666666-0005-0005-0005-000000000005");
        public static readonly Guid VDoc6Id = Guid.Parse("66666666-0006-0006-0006-000000000006");
        public static readonly Guid VDoc7Id = Guid.Parse("66666666-0007-0007-0007-000000000007");
        public static readonly Guid VDoc8Id = Guid.Parse("66666666-0008-0008-0008-000000000008");
        public static readonly Guid VDoc9Id = Guid.Parse("66666666-0009-0009-0009-000000000009");

        // ── Transaction GUIDs ─────────────────────────────────────────────
        public static readonly Guid Txn1Id = Guid.Parse("77777777-0001-0001-0001-000000000001");
        public static readonly Guid Txn2Id = Guid.Parse("77777777-0002-0002-0002-000000000002");
        public static readonly Guid Txn3Id = Guid.Parse("77777777-0003-0003-0003-000000000003");
        public static readonly Guid Txn4Id = Guid.Parse("77777777-0004-0004-0004-000000000004");
        public static readonly Guid Txn5Id = Guid.Parse("77777777-0005-0005-0005-000000000005");
        public static readonly Guid Txn6Id = Guid.Parse("77777777-0006-0006-0006-000000000006");

        // ── ClientAddress GUIDs ───────────────────────────────────────────
        public static readonly Guid ClAddr1Id = Guid.Parse("88888888-0001-0001-0001-000000000001");
        public static readonly Guid ClAddr2Id = Guid.Parse("88888888-0002-0002-0002-000000000002");
        public static readonly Guid ClAddr3Id = Guid.Parse("88888888-0003-0003-0003-000000000003");
        public static readonly Guid ClAddr4Id = Guid.Parse("88888888-0004-0004-0004-000000000004");

        // ── AgencyAddress GUIDs ───────────────────────────────────────────
        public static readonly Guid AgAddr1Id = Guid.Parse("99999999-0001-0001-0001-000000000001");
        public static readonly Guid AgAddr2Id = Guid.Parse("99999999-0002-0002-0002-000000000002");

        // ── CaregiverAddress GUIDs ────────────────────────────────────────
        public static readonly Guid CgAddr1Id = Guid.Parse("aaaabbbb-0001-0001-0001-000000000001");
        public static readonly Guid CgAddr2Id = Guid.Parse("aaaabbbb-0002-0002-0002-000000000002");
        public static readonly Guid CgAddr3Id = Guid.Parse("aaaabbbb-0003-0003-0003-000000000003");
        public static readonly Guid CgAddr4Id = Guid.Parse("aaaabbbb-0004-0004-0004-000000000004");
        public static readonly Guid CgAddr5Id = Guid.Parse("aaaabbbb-0005-0005-0005-000000000005");

        // ── Master entry point ────────────────────────────────────────────
        public static async Task SeedAllAsync(AppDbContext db, UserManager<User> userMgr, RoleManager<IdentityRole<Guid>> roleMgr,
            MongoDbContext mongo, MongoDbSettings mongoSettings)
        {
            // Order matters — respect FK dependencies
            await SeedRolesAsync(roleMgr);
            await SeedUsersAsync(db, userMgr);
            await SeedAgenciesAsync(db);
            await SeedServiceCategoriesAsync(db);
            await SeedCaregiverProfilesAsync(db);
            await SeedClientProfilesAsync(db);
            await SeedCaregiverServicesAsync(db);
            await SeedVerificationDocumentsAsync(db);
            await SeedWalletsAsync(db);
            await SeedFamilyAccessesAsync(db);
            await SeedBookingsAsync(db);
            await SeedTransactionsAsync(db);
            await SeedReviewsAsync(db);
            await SeedNotificationsAsync(db);
            await SeedSupportTicketsAsync(db);
            await SeedTicketMessagesAsync(db);
            await SeedEmergencyAlertsAsync(db);
            await SeedOtpCodesAsync(db);
            await SeedRefreshTokensAsync(db);
            await SeedAddressesAsync(db);
            //await SeedAuditLogsAsync(mongo, mongoSettings);
            //await SeedAgencyActivityLogsAsync(mongo, mongoSettings);
        }

        private static async Task SeedRolesAsync(Microsoft.AspNetCore.Identity.RoleManager<IdentityRole<Guid>> roleMgr)
        {
            if (await roleMgr.Roles.AnyAsync()) return;

            var roles = new[]
            {
                "SuperAdmin","Admin","Client",
                "IndividualCaregiver","AgencyAdmin","AgencyCaregiver","FamilyMember"
            };
            foreach (var role in roles)
                await roleMgr.CreateAsync(new IdentityRole<Guid>(role));
        }

        private static async Task SeedUsersAsync(AppDbContext db, Microsoft.AspNetCore.Identity.UserManager<User> mgr)
        {
            if (await mgr.Users.AnyAsync()) return;

            // Helper: create user + assign role
            async Task<User> Create(Guid id, string first, string last, string email, string phone, UserRole role, string state, string city, string lga, string password, bool emailConfirmed = true)
            {
                var u = new User
                {
                    Id = id,
                    UserName = email,
                    Email = email,
                    FirstName = first,
                    LastName = last,
                    PhoneNumber = phone,
                    Role = role,
                    IsActive = true,
                    EmailConfirmed = emailConfirmed,
                    PhoneNumberConfirmed = true,
                    LockoutEnabled = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await mgr.CreateAsync(u, password);
                await mgr.AddToRoleAsync(u, role.ToString());
                return u;
            }

            // ── SuperAdmin ────────────────────────────────────────────────
            await Create(SuperAdminId, "Admin", "Super", "superadmin@helpathoment.ng", "+2348011111111", UserRole.SuperAdmin, "Lagos", "Lagos Island", "Lagos Island", "Admin@HelpAtHome1!");

            // ── Admins ────────────────────────────────────────────────────
            await Create(Admin1Id, "Chukwuemeka", "Nwosu",
                "admin1@helpathoment.ng", "+2348022222222",
                UserRole.Admin, "Lagos", "Ikeja", "Ikeja",
                "Admin@HelpAtHome1!");

            await Create(Admin2Id, "Fatima", "Bello",
                "admin2@helpathoment.ng", "+2348033333333",
                UserRole.Admin, "Abuja", "Garki", "Garki II",
                "Admin@HelpAtHome1!");

            // ── Agency Admins ─────────────────────────────────────────────
            await Create(AgencyAdmin1Id, "Babatunde", "Adeyemi",
                "agencyadmin1@helpathoment.ng", "+2348044444444",
                UserRole.AgencyAdmin, "Lagos", "Victoria Island", "Eti-Osa",
                "Agency@HelpAtHome1!");

            await Create(AgencyAdmin2Id, "Ngozi", "Obi",
                "agencyadmin2@helpathoment.ng", "+2348055555555",
                UserRole.AgencyAdmin, "Enugu", "Enugu", "Enugu East",
                "Agency@HelpAtHome1!");

            // ── Individual Caregivers ─────────────────────────────────────
            await Create(Caregiver1Id, "Amaka", "Eze",
                "caregiver1@helpathoment.ng", "+2348066666661",
                UserRole.IndividualCaregiver, "Lagos", "Surulere", "Surulere",
                "Care@HelpAtHome1!");

            await Create(Caregiver2Id, "Tunde", "Abiodun",
                "caregiver2@helpathoment.ng", "+2348066666662",
                UserRole.IndividualCaregiver, "Lagos", "Yaba", "Yaba",
                "Care@HelpAtHome1!");

            // ── Agency Caregivers ─────────────────────────────────────────
            await Create(Caregiver3Id, "Blessing", "Okonkwo",
                "caregiver3@helpathoment.ng", "+2348066666663",
                UserRole.AgencyCaregiver, "Lagos", "Lekki", "Eti-Osa",
                "Care@HelpAtHome1!");

            await Create(Caregiver4Id, "Emeka", "Osei",
                "caregiver4@helpathoment.ng", "+2348066666664",
                UserRole.AgencyCaregiver, "Lagos", "Ajah", "Eti-Osa",
                "Care@HelpAtHome1!");

            await Create(Caregiver5Id, "Chidinma", "Udo",
                "caregiver5@helpathoment.ng", "+2348066666665",
                UserRole.AgencyCaregiver, "Enugu", "Enugu", "Enugu North",
                "Care@HelpAtHome1!");

            // ── Clients ───────────────────────────────────────────────────
            await Create(Client1Id, "Mama Ngozi", "Adesanya",
                "client1@helpathoment.ng", "+2348077777771",
                UserRole.Client, "Lagos", "Ikoyi", "Eti-Osa",
                "Client@HelpAtHome1!");

            await Create(Client2Id, "Pa Adewale", "Sanni",
                "client2@helpathoment.ng", "+2348077777772",
                UserRole.Client, "Lagos", "Gbagada", "Kosofe",
                "Client@HelpAtHome1!");

            await Create(Client3Id, "Mrs Ifeoma", "Obi",
                "client3@helpathoment.ng", "+2348077777773",
                UserRole.Client, "Enugu", "Enugu", "Enugu South",
                "Client@HelpAtHome1!");

            await Create(Client4Id, "Chief Bello", "Abdullahi",
                "client4@helpathoment.ng", "+2348077777774",
                UserRole.Client, "Abuja", "Wuse", "Wuse II",
                "Client@HelpAtHome1!");

            // ── Family Members ────────────────────────────────────────────
            await Create(Family1Id, "Kemi", "Adesanya",
                "family1@helpathoment.ng", "+2348088888881",
                UserRole.FamilyMember, "Lagos", "Lekki", "Eti-Osa",
                "Family@HelpAtHome1!");

            await Create(Family2Id, "Tolu", "Sanni",
                "family2@helpathoment.ng", "+2348088888882",
                UserRole.FamilyMember, "Abuja", "Maitama", "Municipal",
                "Family@HelpAtHome1!");
        }

        private static async Task SeedAgenciesAsync(AppDbContext db)
        {
            if (await db.Agencies.AnyAsync()) return;

            db.Agencies.AddRange(
                new Agency
                {
                    Id = Agency1Id,
                    AgencyAdminUserId = AgencyAdmin1Id,
                    AgencyName = "CareBridge Services Ltd",
                    RegistrationNumber = "RC-1234567",
                    Email = "info@carebridge.ng",
                    PhoneNumber = "+2348044444444",
                    LogoUrl = "https://res.cloudinary.com/helpath/image/upload/agency1-logo.png",
                    Description = "Professional elderly care and household support agency in Lagos.",
                    Website = "https://carebridge.ng",
                    VerificationStatus = VerificationStatus.Approved,
                    IsActive = true,
                    CommissionRate = 15,
                    AgencyCommissionRate = 10,
                    WalletBalance = 15000m,
                    TotalCaregiversCount = 3,
                    VerifiedAt = DateTime.UtcNow.AddMonths(-3),
                    CreatedAt = DateTime.UtcNow.AddMonths(-4),
                    UpdatedAt = DateTime.UtcNow
                },
                new Agency
                {
                    Id = Agency2Id,
                    AgencyAdminUserId = AgencyAdmin2Id,
                    AgencyName = "HomeCare Enugu Ltd",
                    RegistrationNumber = "RC-7654321",
                    Email = "info@homecareenugu.ng",
                    PhoneNumber = "+2348055555555",
                    Description = "Trusted homecare agency serving Enugu and environs.",
                    VerificationStatus = VerificationStatus.Pending,
                    IsActive = false,
                    CommissionRate = 15,
                    AgencyCommissionRate = 10,
                    WalletBalance = 0m,
                    TotalCaregiversCount = 0,
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    UpdatedAt = DateTime.UtcNow
                }
            );
            await db.SaveChangesAsync();
        }

        private static async Task SeedServiceCategoriesAsync(AppDbContext db)
        {
            if (await db.ServiceCategories.AnyAsync()) return;

            db.ServiceCategories.AddRange(
                new ServiceCategory
                {
                    Id = CatElderly,
                    Name = "Elderly Care",
                    Description = "Dedicated care for elderly individuals including personal hygiene, mobility assistance, and companionship.",
                    IsActive = true,
                    SortOrder = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },

                new ServiceCategory
                {
                    Id = CatCooking,
                    Name = "Cooking & Meal Preparation",
                    Description = "Home cooking, grocery shopping, and meal planning for all dietary needs.",
                    IsActive = true,
                    SortOrder = 2,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },

                new ServiceCategory
                {
                    Id = CatCleaning,
                    Name = "House Cleaning",
                    Description = "Thorough house cleaning, laundry, ironing, and general tidying.",
                    IsActive = true,
                    SortOrder = 3,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },

                new ServiceCategory
                {
                    Id = CatErrands,
                    Name = "Errands & Shopping",
                    Description = "Market runs, bill payments, pharmacy pickups, and general errands.",
                    IsActive = true,
                    SortOrder = 4,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },

                new ServiceCategory
                {
                    Id = CatMedication,
                    Name = "Medication Administration",
                    Description = "Assistance with medication schedules, reminders, and basic health monitoring.",
                    IsActive = true,
                    SortOrder = 5,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },

                new ServiceCategory
                {
                    Id = CatCompanion,
                    Name = "Companionship",
                    Description = "Social visits, conversation, light entertainment, and emotional support.",
                    IsActive = true,
                    SortOrder = 6,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );
            await db.SaveChangesAsync();
        }

        private static async Task SeedCaregiverProfilesAsync(AppDbContext db)
        {
            if (await db.CaregiverProfiles.AnyAsync()) return;

            db.CaregiverProfiles.AddRange(
                // Individual — Verified, Elite badge
                new CaregiverProfile
                {
                    Id = CgProfile1Id,
                    UserId = Caregiver1Id,
                    AgencyId = null,
                    Bio = "Experienced elderly care specialist with 6 years working in Lagos hospitals and private homes.",
                    YearsOfExperience = 6,
                    Badge = BadgeLevel.Elite,
                    HourlyRate = 2500m,
                    DailyRate = 15000m,
                    MonthlyRate = 120000m,
                    GenderProvided = Gender.Female,
                    Services = Services.CanCook | Services.CanDoErrands | Services.CanProvideCompanionship | Services.CanDoHeavyCleaning | Services.CanDoErrands | Services.CanDriveClient | Services.HasFirstAidCertificate,
                    LanguagesSpoken = "[\"English\",\"Igbo\",\"Pidgin\"]",
                    WorkingHours = "{\"Mon\":\"08:00 - 18:00\",\"Tue\":\"08:00 - 18:00\",\"Wed\":\"08:00 - 18:00\",\"Thu\":\"08:00 - 18:00\",\"Fri\":\"08:00 - 18:00\"}",
                    IsAvailable = true,
                    VerificationStatus = VerificationStatus.Approved,
                    VerifiedAt = DateTime.UtcNow.AddMonths(-2),
                    VerifiedByAdminId = Admin1Id.ToString(),
                    IsBackgroundChecked = true,
                    BackgroundCheckDate = DateTime.UtcNow.AddMonths(-2),
                    TotalCompletedBookings = 28,
                    AverageRating = 4.7m,
                    TotalReviews = 24,
                    CreatedAt = DateTime.UtcNow.AddMonths(-6),
                    UpdatedAt = DateTime.UtcNow
                },

                // Individual — Verified badge
                new CaregiverProfile
                {
                    Id = CgProfile2Id,
                    UserId = Caregiver2Id,
                    AgencyId = null,
                    Bio = "Dedicated male caregiver specialising in companionship and errands for elderly men.",
                    YearsOfExperience = 3,
                    Badge = BadgeLevel.Verified,
                    HourlyRate = 2000m,
                    DailyRate = 12000m,
                    MonthlyRate = 90000m,
                    GenderProvided = Gender.Male,
                    Services = Services.CanCook | Services.CanDoErrands | Services.CanProvideCompanionship | Services.CanDoHeavyCleaning | Services.CanCareForBedridden | Services.CanDoErrands | Services.CanDriveClient | Services.HasFirstAidCertificate,
                    LanguagesSpoken = "[\"English\",\"Yoruba\"]",
                    IsAvailable = true,
                    VerificationStatus = VerificationStatus.Approved,
                    VerifiedAt = DateTime.UtcNow.AddMonths(-1),
                    VerifiedByAdminId = Admin1Id.ToString(),
                    IsBackgroundChecked = true,
                    TotalCompletedBookings = 8,
                    AverageRating = 4.4m,
                    TotalReviews = 7,
                    CreatedAt = DateTime.UtcNow.AddMonths(-3),
                    UpdatedAt = DateTime.UtcNow
                },

                // Agency 1 — Champion badge
                new CaregiverProfile
                {
                    Id = CgProfile3Id,
                    UserId = Caregiver3Id,
                    AgencyId = Agency1Id,
                    Bio = "Highly experienced senior carer with nursing background. Specialist in post-hospitalisation home recovery.",
                    YearsOfExperience = 9,
                    Badge = BadgeLevel.Champion,
                    HourlyRate = 3000m,
                    DailyRate = 18000m,
                    MonthlyRate = 150000m,
                    GenderProvided = Gender.Female,
                    Services = Services.CanCook | Services.CanDoErrands | Services.CanProvideCompanionship | Services.CanDoHeavyCleaning | Services.CanCareForBedridden | Services.CanDoErrands,
                    LanguagesSpoken = "[\"English\",\"Igbo\",\"Yoruba\"]",
                    IsAvailable = true,
                    VerificationStatus = VerificationStatus.Approved,
                    VerifiedAt = DateTime.UtcNow.AddMonths(-5),
                    VerifiedByAdminId = Admin1Id.ToString(),
                    IsBackgroundChecked = true,
                    TotalCompletedBookings = 62,
                    AverageRating = 4.9m,
                    TotalReviews = 58,
                    CreatedAt = DateTime.UtcNow.AddMonths(-10),
                    UpdatedAt = DateTime.UtcNow
                },

                // Agency 1 — New badge
                new CaregiverProfile
                {
                    Id = CgProfile4Id,
                    UserId = Caregiver4Id,
                    AgencyId = Agency1Id,
                    Bio = "New caregiver eager to assist elderly clients with daily routines and errands.",
                    YearsOfExperience = 1,
                    Badge = BadgeLevel.New,
                    HourlyRate = 1500m,
                    DailyRate = 9000m,
                    MonthlyRate = 70000m,
                    GenderProvided = Gender.Male,
                    Services = Services.CanCook | Services.CanDoErrands | Services.CanProvideCompanionship | Services.CanDoHeavyCleaning | Services.CanCareForBedridden | Services.CanDriveClient | Services.CanCareForBedridden,
                    LanguagesSpoken = "[\"English\",\"Igbo\"]",
                    IsAvailable = true,
                    VerificationStatus = VerificationStatus.Approved,
                    VerifiedAt = DateTime.UtcNow.AddDays(-14),
                    VerifiedByAdminId = Admin2Id.ToString(),
                    IsBackgroundChecked = false,
                    TotalCompletedBookings = 2,
                    AverageRating = 4.0m,
                    TotalReviews = 2,
                    CreatedAt = DateTime.UtcNow.AddMonths(-1),
                    UpdatedAt = DateTime.UtcNow
                },

                // Agency 1 — Pending verification
                new CaregiverProfile
                {
                    Id = CgProfile5Id,
                    UserId = Caregiver5Id,
                    AgencyId = Agency1Id,
                    Bio = "Caring and patient individual with passion for elderly care in Enugu.",
                    YearsOfExperience = 2,
                    Badge = BadgeLevel.New,
                    HourlyRate = 1800m,
                    DailyRate = 10000m,
                    MonthlyRate = 80000m,
                    GenderProvided = Gender.Female,
                    Services = Services.CanCook | Services.CanDoErrands | Services.CanProvideCompanionship | Services.CanDoHeavyCleaning | Services.CanCareForBedridden,
                    LanguagesSpoken = "[\"English\",\"Igbo\"]",
                    IsAvailable = false,
                    VerificationStatus = VerificationStatus.Pending,
                    TotalCompletedBookings = 0,
                    AverageRating = 0m,
                    TotalReviews = 0,
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    UpdatedAt = DateTime.UtcNow
                }
            );
            await db.SaveChangesAsync();
        }

        private static async Task SeedClientProfilesAsync(AppDbContext db)
        {
            if (await db.ClientProfiles.AnyAsync()) return;

            db.ClientProfiles.AddRange(
                new ClientProfile
                {
                    Id = ClientProfile1Id,
                    UserId = Client1Id,
                    SpecialNotes = "Elderly woman (74 yrs). Lives alone in Ikoyi. Has mild arthritis. Prefers female caregivers.",
                    MedicalConditions = "Arthritis, hypertension",
                    DateOfBirth = new DateTime(1978, 2, 23, 13, 45, 33),
                    Gender = Gender.Female,
                    CareGiverGenderPreference = PreferedGender.NoPreference,
                    RequireVerifiedOnly = true,
                    WalletBalance = 45000m,
                    CreatedAt = DateTime.UtcNow.AddMonths(-5),
                    UpdatedAt = DateTime.UtcNow,
                    ServicesNeeded = Services.CanCareForBedridden | Services.CanProvideCompanionship | Services.HasFirstAidCertificate,
                    EmergencyContactName = "Kabiru Dada",
                    EmergencyContactPhoneNumber = "+2348088888881",
                    Frequency = Frequency.Daily,
                    RelationToRecipient = RelationToRecipient.Self
                },
                new ClientProfile
                {
                    Id = ClientProfile2Id,
                    UserId = Client2Id,
                    SpecialNotes = "Retired man (78 yrs). Recovering from stroke. Needs mobility assistance and medication reminders.",
                    MedicalConditions = "Stroke recovery, hypertension",
                    DateOfBirth = new DateTime(1961, 8, 21, 13, 45, 33),
                    Gender = Gender.Male,
                    CareGiverGenderPreference = PreferedGender.NoPreference,
                    RequireVerifiedOnly = true,
                    WalletBalance = 30000m,
                    CreatedAt = DateTime.UtcNow.AddMonths(-4),
                    UpdatedAt = DateTime.UtcNow,
                    ServicesNeeded = Services.CanCareForBedridden | Services.CanProvideCompanionship | Services.CanDoErrands | Services.HasFirstAidCertificate,
                    EmergencyContactName = "Tolu Sanni",
                    EmergencyContactPhoneNumber = "+2348088888882",
                    Frequency = Frequency.Weekly,
                    RelationToRecipient = RelationToRecipient.Self
                },
                new ClientProfile
                {
                    Id = ClientProfile3Id,
                    UserId = Client3Id,
                    SpecialNotes = "Elderly woman (68 yrs). Wants help with cooking and house cleaning twice a week.",
                    MedicalConditions = "Diabetes",
                    DateOfBirth = new DateTime(1990, 12, 13, 13, 45, 33),
                    Gender = Gender.Female,
                    CareGiverGenderPreference = PreferedGender.Female,
                    RequireVerifiedOnly = false,
                    WalletBalance = 20000m,
                    CreatedAt = DateTime.UtcNow.AddMonths(-2),
                    UpdatedAt = DateTime.UtcNow,
                    ServicesNeeded = Services.CanCareForBedridden | Services.CanProvideCompanionship | Services.CanDoErrands | Services.HasFirstAidCertificate | Services.CanCook,
                    EmergencyContactName = "Igbo Edo",
                    EmergencyContactPhoneNumber = "+2348088888883",
                    Frequency = Frequency.OneTime,
                    RelationToRecipient = RelationToRecipient.Mother
                },
                new ClientProfile
                {
                    Id = ClientProfile4Id,
                    UserId = Client4Id,
                    SpecialNotes = "Active retired man (70 yrs). Needs help with errands and market runs.",
                    DateOfBirth = new DateTime(1971, 11, 29, 13, 45, 33),
                    Gender = Gender.Male,
                    CareGiverGenderPreference = PreferedGender.Male,
                    RequireVerifiedOnly = false,
                    WalletBalance = 55000m,
                    CreatedAt = DateTime.UtcNow.AddMonths(-1),
                    UpdatedAt = DateTime.UtcNow,
                    ServicesNeeded = Services.CanCareForBedridden | Services.CanProvideCompanionship | Services.CanDoErrands | Services.HasFirstAidCertificate | Services.CanAdministerMedication,
                    EmergencyContactName = "Ilorin Ibukun",
                    EmergencyContactPhoneNumber = "+2348088888884",
                    Frequency = Frequency.Daily,
                    RelationToRecipient = RelationToRecipient.Self
                }
            );
            await db.SaveChangesAsync();
        }

        private static async Task SeedCaregiverServicesAsync(AppDbContext db)
        {
            if (await db.CaregiverServices.AnyAsync()) return;

            db.CaregiverServices.AddRange(
                // Caregiver 1 — Elderly Care, Cooking, Medication
                new CaregiverService { Id = Guid.NewGuid(), CaregiverProfileId = CgProfile1Id, ServiceCategoryId = CatElderly, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new CaregiverService { Id = Guid.NewGuid(), CaregiverProfileId = CgProfile1Id, ServiceCategoryId = CatCooking, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new CaregiverService { Id = Guid.NewGuid(), CaregiverProfileId = CgProfile1Id, ServiceCategoryId = CatMedication, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                // Caregiver 2 — Companion, Errands
                new CaregiverService { Id = Guid.NewGuid(), CaregiverProfileId = CgProfile2Id, ServiceCategoryId = CatCompanion, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new CaregiverService { Id = Guid.NewGuid(), CaregiverProfileId = CgProfile2Id, ServiceCategoryId = CatErrands, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                // Caregiver 3 — Elderly Care, Medication, Cleaning, Cooking
                new CaregiverService { Id = Guid.NewGuid(), CaregiverProfileId = CgProfile3Id, ServiceCategoryId = CatElderly, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new CaregiverService { Id = Guid.NewGuid(), CaregiverProfileId = CgProfile3Id, ServiceCategoryId = CatMedication, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                // Caregiver 4 — Cleaning, Errands
                new CaregiverService { Id = Guid.NewGuid(), CaregiverProfileId = CgProfile4Id, ServiceCategoryId = CatCleaning, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new CaregiverService { Id = Guid.NewGuid(), CaregiverProfileId = CgProfile4Id, ServiceCategoryId = CatErrands, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                // Caregiver 5 — Companion, Cooking
                new CaregiverService { Id = Guid.NewGuid(), CaregiverProfileId = CgProfile5Id, ServiceCategoryId = CatCompanion, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            );
            await db.SaveChangesAsync();
        }

        private static async Task SeedVerificationDocumentsAsync(AppDbContext db)
        {
            if (await db.VerificationDocuments.AnyAsync()) return;

            db.VerificationDocuments.AddRange(
                // Caregiver 1 — National ID (Approved)
                new VerificationDocument
                {
                    Id = VDoc1Id,
                    CaregiverProfileId = CgProfile1Id,
                    DocumentType = DocumentType.NationalId,
                    DocumentUrl = "https://res.cloudinary.com/helpath/image/upload/cg1-nationalid.jpg",
                    DocumentNumber = "AB123456789",
                    Status = VerificationStatus.Approved,
                    ReviewNote = "ID is clear and valid.",
                    ReviewedByAdminId = Admin1Id,
                    ReviewedAt = DateTime.UtcNow.AddMonths(-2),
                    CreatedAt = DateTime.UtcNow.AddMonths(-2),
                    UpdatedAt = DateTime.UtcNow
                },
                // Caregiver 1 — Police Clearance (Approved)
                new VerificationDocument
                {
                    Id = VDoc2Id,
                    CaregiverProfileId = CgProfile1Id,
                    DocumentType = DocumentType.PoliceReport,
                    DocumentUrl = "https://res.cloudinary.com/helpath/image/upload/cg1-police.jpg",
                    DocumentNumber = "PCR-2023-0042",
                    Status = VerificationStatus.Approved,
                    ReviewNote = "Police report valid and recent.",
                    ReviewedByAdminId = Admin1Id,
                    ReviewedAt = DateTime.UtcNow.AddMonths(-2),
                    CreatedAt = DateTime.UtcNow.AddMonths(-2),
                    UpdatedAt = DateTime.UtcNow
                },
                // Caregiver 2 — National ID (Approved)
                new VerificationDocument
                {
                    Id = VDoc3Id,
                    CaregiverProfileId = CgProfile2Id,
                    DocumentType = DocumentType.NationalId,
                    DocumentUrl = "https://res.cloudinary.com/helpath/image/upload/cg2-nationalid.jpg",
                    DocumentNumber = "CD987654321",
                    Status = VerificationStatus.Approved,
                    ReviewNote = "ID verified.",
                    ReviewedByAdminId = Admin1Id,
                    ReviewedAt = DateTime.UtcNow.AddMonths(-1),
                    CreatedAt = DateTime.UtcNow.AddMonths(-1),
                    UpdatedAt = DateTime.UtcNow
                },
                // Caregiver 3 — National ID (Approved)
                new VerificationDocument
                {
                    Id = VDoc4Id,
                    CaregiverProfileId = CgProfile3Id,
                    DocumentType = DocumentType.NationalId,
                    DocumentUrl = "https://res.cloudinary.com/helpath/image/upload/cg3-nationalid.jpg",
                    Status = VerificationStatus.Approved,
                    ReviewedByAdminId = Admin1Id,
                    ReviewedAt = DateTime.UtcNow.AddMonths(-5),
                    CreatedAt = DateTime.UtcNow.AddMonths(-5),
                    UpdatedAt = DateTime.UtcNow
                },
                // Caregiver 3 — Nursing Certificate (Approved)
                new VerificationDocument
                {
                    Id = VDoc5Id,
                    CaregiverProfileId = CgProfile3Id,
                    DocumentType = DocumentType.Certificate,
                    DocumentUrl = "https://res.cloudinary.com/helpath/image/upload/cg3-cert.jpg",
                    DocumentNumber = "NMCN-2019-0890",
                    Status = VerificationStatus.Approved,
                    ReviewNote = "Nursing council cert confirmed valid.",
                    ReviewedByAdminId = Admin1Id,
                    ReviewedAt = DateTime.UtcNow.AddMonths(-5),
                    CreatedAt = DateTime.UtcNow.AddMonths(-5),
                    UpdatedAt = DateTime.UtcNow
                },
                // Caregiver 4 — National ID (Approved)
                new VerificationDocument
                {
                    Id = VDoc6Id,
                    CaregiverProfileId = CgProfile4Id,
                    DocumentType = DocumentType.NationalId,
                    DocumentUrl = "https://res.cloudinary.com/helpath/image/upload/cg4-nationalid.jpg",
                    Status = VerificationStatus.Approved,
                    ReviewedByAdminId = Admin2Id,
                    ReviewedAt = DateTime.UtcNow.AddDays(-14),
                    CreatedAt = DateTime.UtcNow.AddDays(-14),
                    UpdatedAt = DateTime.UtcNow
                },
                // Caregiver 5 — National ID (Pending)
                new VerificationDocument
                {
                    Id = VDoc7Id,
                    CaregiverProfileId = CgProfile5Id,
                    DocumentType = DocumentType.NationalId,
                    DocumentUrl = "https://res.cloudinary.com/helpath/image/upload/cg5-nationalid.jpg",
                    Status = VerificationStatus.Pending,
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    UpdatedAt = DateTime.UtcNow
                },
                // Agency 1 — Registration (Approved)
                new VerificationDocument
                {
                    Id = VDoc8Id,
                    AgencyId = Agency1Id,
                    DocumentType = DocumentType.AgencyRegistration,
                    DocumentUrl = "https://res.cloudinary.com/helpath/image/upload/agency1-regdoc.jpg",
                    DocumentNumber = "RC-1234567",
                    Status = VerificationStatus.Approved,
                    ReviewNote = "CAC registration confirmed.",
                    ReviewedByAdminId = Admin1Id,
                    ReviewedAt = DateTime.UtcNow.AddMonths(-3),
                    CreatedAt = DateTime.UtcNow.AddMonths(-4),
                    UpdatedAt = DateTime.UtcNow
                },
                // Agency 2 — Registration (Pending)
                new VerificationDocument
                {
                    Id = VDoc9Id,
                    AgencyId = Agency2Id,
                    DocumentType = DocumentType.AgencyRegistration,
                    DocumentUrl = "https://res.cloudinary.com/helpath/image/upload/agency2-regdoc.jpg",
                    DocumentNumber = "RC-7654321",
                    Status = VerificationStatus.Pending,
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    UpdatedAt = DateTime.UtcNow
                }
            );
            await db.SaveChangesAsync();
        }

        private static async Task SeedWalletsAsync(AppDbContext db)
        {
            if (await db.Wallets.AnyAsync()) return;

            var now = DateTime.UtcNow;
            db.Wallets.AddRange(
                new Wallet { Id = Wallet_SA, UserId = SuperAdminId, Balance = 0m, TotalEarned = 0m, TotalSpent = 0m, TotalWithdrawn = 0m, CreatedAt = now, UpdatedAt = now },
                new Wallet { Id = Wallet_A1, UserId = Admin1Id, Balance = 0m, TotalEarned = 0m, TotalSpent = 0m, TotalWithdrawn = 0m, CreatedAt = now, UpdatedAt = now },
                new Wallet { Id = Wallet_A2, UserId = Admin2Id, Balance = 0m, TotalEarned = 0m, TotalSpent = 0m, TotalWithdrawn = 0m, CreatedAt = now, UpdatedAt = now },
                new Wallet { Id = Wallet_AA1, UserId = AgencyAdmin1Id, Balance = 15000m, TotalEarned = 45000m, TotalSpent = 0m, TotalWithdrawn = 30000m, CreatedAt = now, UpdatedAt = now },
                new Wallet { Id = Wallet_AA2, UserId = AgencyAdmin2Id, Balance = 0m, TotalEarned = 0m, TotalSpent = 0m, TotalWithdrawn = 0m, CreatedAt = now, UpdatedAt = now },
                new Wallet { Id = Wallet_CG1, UserId = Caregiver1Id, Balance = 38500m, TotalEarned = 120000m, TotalSpent = 0m, TotalWithdrawn = 81500m, CreatedAt = now, UpdatedAt = now },
                new Wallet { Id = Wallet_CG2, UserId = Caregiver2Id, Balance = 12000m, TotalEarned = 25000m, TotalSpent = 0m, TotalWithdrawn = 13000m, CreatedAt = now, UpdatedAt = now },
                new Wallet { Id = Wallet_CG3, UserId = Caregiver3Id, Balance = 62000m, TotalEarned = 280000m, TotalSpent = 0m, TotalWithdrawn = 218000m, CreatedAt = now, UpdatedAt = now },
                new Wallet { Id = Wallet_CG4, UserId = Caregiver4Id, Balance = 5400m, TotalEarned = 9000m, TotalSpent = 0m, TotalWithdrawn = 3600m, CreatedAt = now, UpdatedAt = now },
                new Wallet { Id = Wallet_CG5, UserId = Caregiver5Id, Balance = 0m, TotalEarned = 0m, TotalSpent = 0m, TotalWithdrawn = 0m, CreatedAt = now, UpdatedAt = now },
                new Wallet { Id = Wallet_CL1, UserId = Client1Id, Balance = 45000m, TotalEarned = 100000m, TotalSpent = 55000m, TotalWithdrawn = 0m, CreatedAt = now, UpdatedAt = now },
                new Wallet { Id = Wallet_CL2, UserId = Client2Id, Balance = 30000m, TotalEarned = 60000m, TotalSpent = 30000m, TotalWithdrawn = 0m, CreatedAt = now, UpdatedAt = now },
                new Wallet { Id = Wallet_CL3, UserId = Client3Id, Balance = 20000m, TotalEarned = 35000m, TotalSpent = 15000m, TotalWithdrawn = 0m, CreatedAt = now, UpdatedAt = now },
                new Wallet { Id = Wallet_CL4, UserId = Client4Id, Balance = 55000m, TotalEarned = 80000m, TotalSpent = 25000m, TotalWithdrawn = 0m, CreatedAt = now, UpdatedAt = now }
            );
            await db.SaveChangesAsync();
        }

        private static async Task SeedFamilyAccessesAsync(AppDbContext db)
        {
            if (await db.FamilyAccesses.AnyAsync()) return;

            db.FamilyAccesses.AddRange(
                // Kemi monitors her mother Mama Ngozi (Client1) — Approved, FullAccess
                new FamilyAccess
                {
                    Id = FAccess1Id,
                    ClientUserId = Client1Id,
                    FamilyMemberUserId = Family1Id,
                    AccessLevel = AccessLevel.FullAccess,
                    IsApproved = true,
                    ApprovedAt = DateTime.UtcNow.AddMonths(-4),
                    ReceiveEmergencyAlerts = true,
                    ReceiveBookingUpdates = true,
                    ReceivePaymentAlerts = true,
                    CreatedAt = DateTime.UtcNow.AddMonths(-4),
                    UpdatedAt = DateTime.UtcNow
                },
                // Tolu monitors his father Pa Adewale (Client2) — Approved, ViewOnly
                new FamilyAccess
                {
                    Id = FAccess2Id,
                    ClientUserId = Client2Id,
                    FamilyMemberUserId = Family2Id,
                    AccessLevel = AccessLevel.ViewOnly,
                    IsApproved = true,
                    ApprovedAt = DateTime.UtcNow.AddMonths(-3),
                    ReceiveEmergencyAlerts = true,
                    ReceiveBookingUpdates = true,
                    ReceivePaymentAlerts = false,
                    CreatedAt = DateTime.UtcNow.AddMonths(-3),
                    UpdatedAt = DateTime.UtcNow
                },
                // Kemi also invited to monitor Client2 — Pending approval
                new FamilyAccess
                {
                    Id = FAccess3Id,
                    ClientUserId = Client2Id,
                    FamilyMemberUserId = Family1Id,
                    AccessLevel = AccessLevel.Notify,
                    IsApproved = false,
                    ReceiveEmergencyAlerts = true,
                    ReceiveBookingUpdates = false,
                    ReceivePaymentAlerts = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    UpdatedAt = DateTime.UtcNow
                },
                // Tolu invited to monitor Client3 — Approved
                new FamilyAccess
                {
                    Id = FAccess4Id,
                    ClientUserId = Client3Id,
                    FamilyMemberUserId = Family2Id,
                    AccessLevel = AccessLevel.ViewOnly,
                    IsApproved = true,
                    ApprovedAt = DateTime.UtcNow.AddDays(-7),
                    ReceiveEmergencyAlerts = true,
                    ReceiveBookingUpdates = true,
                    ReceivePaymentAlerts = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-8),
                    UpdatedAt = DateTime.UtcNow
                }
            );
            await db.SaveChangesAsync();
        }

        private static async Task SeedBookingsAsync(AppDbContext db)
        {
            if (await db.Bookings.AnyAsync()) return;

            var now = DateTime.UtcNow;
            db.Bookings.AddRange(
                // Booking 1 — Completed (Client1 & Caregiver1, Elderly Care)
                new Booking
                {
                    Id = Booking1Id,
                    BookingReference = "HAH-20240801-0001",
                    ClientProfileId = ClientProfile1Id,
                    CaregiverProfileId = CgProfile1Id,
                    ServiceCategoryId = CatElderly,
                    Status = BookingStatus.Completed,
                    Frequency = FrequencyType.Monthly,
                    ScheduledStartDate = now.AddMonths(-3),
                    ScheduledEndDate = now.AddMonths(-2),
                    DailyStartTime = new TimeSpan(8, 0, 0),
                    DailyEndTime = new TimeSpan(17, 0, 0),
                    AgreedAmount = 120000m,
                    PlatformFee = 18000m,
                    CaregiverEarnings = 102000m,
                    PaymentStatus = PaymentStatus.Paid,
                    SpecialInstructions = "Please assist with morning routine and medication at 9am and 9pm.",
                    ClientAddress = "14 Glover Road, Ikoyi, Lagos",
                    ClientLatitude = 6.4550m,
                    ClientLongitude = 3.4280m,
                    IsReviewedByClient = true,
                    IsReviewedByCaregiver = true,
                    HasDispute = false,
                    AcceptedAt = now.AddMonths(-3).AddHours(2),
                    StartedAt = now.AddMonths(-3).AddHours(25),
                    CompletedAt = now.AddMonths(-2),
                    CreatedAt = now.AddMonths(-3),
                    UpdatedAt = now
                },

                // Booking 2 — InProgress (Client2 & Caregiver3, Medication)
                new Booking
                {
                    Id = Booking2Id,
                    BookingReference = "HAH-20240901-0002",
                    ClientProfileId = ClientProfile2Id,
                    CaregiverProfileId = CgProfile3Id,
                    ServiceCategoryId = CatMedication,
                    Status = BookingStatus.InProgress,
                    Frequency = FrequencyType.Weekly,
                    ScheduledStartDate = now.AddDays(-5),
                    ScheduledEndDate = now.AddDays(25),
                    DailyStartTime = new TimeSpan(7, 0, 0),
                    DailyEndTime = new TimeSpan(19, 0, 0),
                    AgreedAmount = 60000m,
                    PlatformFee = 9000m,
                    CaregiverEarnings = 51000m,
                    PaymentStatus = PaymentStatus.Paid,
                    SpecialInstructions = "Patient had stroke. Handle with care. Blood pressure check every morning.",
                    ClientAddress = "22 Opebi Road, Gbagada, Lagos",
                    ClientLatitude = 6.5480m,
                    ClientLongitude = 3.3590m,
                    IsReviewedByClient = false,
                    IsReviewedByCaregiver = false,
                    HasDispute = false,
                    AcceptedAt = now.AddDays(-5).AddHours(1),
                    StartedAt = now.AddDays(-5).AddHours(2),
                    CreatedAt = now.AddDays(-6),
                    UpdatedAt = now
                },

                // Booking 3 — Accepted (Client3 & Caregiver1, Cooking)
                new Booking
                {
                    Id = Booking3Id,
                    BookingReference = "HAH-20240915-0003",
                    ClientProfileId = ClientProfile3Id,
                    CaregiverProfileId = CgProfile1Id,
                    ServiceCategoryId = CatCooking,
                    Status = BookingStatus.Accepted,
                    Frequency = FrequencyType.Weekly,
                    ScheduledStartDate = now.AddDays(2),
                    ScheduledEndDate = now.AddDays(30),
                    DailyStartTime = new TimeSpan(10, 0, 0),
                    DailyEndTime = new TimeSpan(15, 0, 0),
                    AgreedAmount = 50000m,
                    PlatformFee = 7500m,
                    CaregiverEarnings = 42500m,
                    PaymentStatus = PaymentStatus.Paid,
                    ClientAddress = "7 Abakaliki Road, Enugu South",
                    IsReviewedByClient = false,
                    IsReviewedByCaregiver = false,
                    HasDispute = false,
                    AcceptedAt = now.AddHours(-6),
                    CreatedAt = now.AddDays(-1),
                    UpdatedAt = now
                },

                // Booking 4 — Disputed (Client4 & Caregiver2, Errands)
                new Booking
                {
                    Id = Booking4Id,
                    BookingReference = "HAH-20240910-0004",
                    ClientProfileId = ClientProfile4Id,
                    CaregiverProfileId = CgProfile2Id,
                    ServiceCategoryId = CatErrands,
                    Status = BookingStatus.Disputed,
                    Frequency = FrequencyType.OneTime,
                    ScheduledStartDate = now.AddDays(-8),
                    ScheduledEndDate = now.AddDays(-7),
                    AgreedAmount = 12000m,
                    PlatformFee = 1800m,
                    CaregiverEarnings = 10200m,
                    PaymentStatus = PaymentStatus.Paid,
                    SpecialInstructions = "Buy provisions from Shoprite and pay NEPA bill.",
                    ClientAddress = "Plot 5, Wuse Zone 4, Abuja",
                    IsReviewedByClient = false,
                    IsReviewedByCaregiver = false,
                    HasDispute = true,
                    AcceptedAt = now.AddDays(-8).AddHours(1),
                    StartedAt = now.AddDays(-8).AddHours(4),
                    CancellationReason = null,
                    CreatedAt = now.AddDays(-9),
                    UpdatedAt = now
                },

                // Booking 5 — Pending (Client1 & Caregiver4, Cleaning)
                new Booking
                {
                    Id = Booking5Id,
                    BookingReference = "HAH-20240920-0005",
                    ClientProfileId = ClientProfile1Id,
                    CaregiverProfileId = CgProfile4Id,
                    ServiceCategoryId = CatCleaning,
                    Status = BookingStatus.Pending,
                    Frequency = FrequencyType.OneTime,
                    ScheduledStartDate = now.AddDays(1),
                    ScheduledEndDate = now.AddDays(1).AddHours(8),
                    AgreedAmount = 9000m,
                    PlatformFee = 1350m,
                    CaregiverEarnings = 7650m,
                    PaymentStatus = PaymentStatus.Paid,
                    ClientAddress = "14 Glover Road, Ikoyi, Lagos",
                    IsReviewedByClient = false,
                    IsReviewedByCaregiver = false,
                    HasDispute = false,
                    CreatedAt = now,
                    UpdatedAt = now
                }
            );
            await db.SaveChangesAsync();
        }

        private static async Task SeedTransactionsAsync(AppDbContext db)
        {
            if (await db.Transactions.AnyAsync()) return;

            var now = DateTime.UtcNow;
            db.Transactions.AddRange(
                // Client1 deposit
                new Transaction
                {
                    Id = Txn1Id,
                    TransactionReference = "TXN20240801001",
                    WalletId = Wallet_CL1,
                    Type = TransactionType.Deposit,
                    Status = TransactionStatus.Success,
                    Amount = 100000m,
                    BalanceBefore = 0m,
                    BalanceAfter = 100000m,
                    PaystackReference = "pay_abc123def456",
                    PaystackTransactionId = "1234567890",
                    Description = "Wallet top-up via Paystack",
                    CreatedAt = now.AddMonths(-4),
                    UpdatedAt = now
                },
                // Client1 booking deduction (Booking1)
                new Transaction
                {
                    Id = Txn2Id,
                    TransactionReference = "TXN20240801002",
                    WalletId = Wallet_CL1,
                    BookingId = Booking1Id,
                    Type = TransactionType.Booking,
                    Status = TransactionStatus.Success,
                    Amount = -120000m,
                    BalanceBefore = 100000m,
                    BalanceAfter = -20000m,
                    Description = "Booking HAH-20240801-0001",
                    CreatedAt = now.AddMonths(-3),
                    UpdatedAt = now
                },
                // Caregiver1 payout from Booking1
                new Transaction
                {
                    Id = Txn3Id,
                    TransactionReference = "TXN20240802001",
                    WalletId = Wallet_CG1,
                    BookingId = Booking1Id,
                    Type = TransactionType.Payout,
                    Status = TransactionStatus.Success,
                    Amount = 102000m,
                    BalanceBefore = 0m,
                    BalanceAfter = 102000m,
                    Description = "Earnings released: HAH-20240801-0001",
                    CreatedAt = now.AddMonths(-2),
                    UpdatedAt = now
                },
                // Client2 deposit
                new Transaction
                {
                    Id = Txn4Id,
                    TransactionReference = "TXN20240901001",
                    WalletId = Wallet_CL2,
                    Type = TransactionType.Deposit,
                    Status = TransactionStatus.Success,
                    Amount = 60000m,
                    BalanceBefore = 0m,
                    BalanceAfter = 60000m,
                    PaystackReference = "pay_xyz789uvw012",
                    Description = "Wallet top-up via Paystack",
                    CreatedAt = now.AddDays(-6),
                    UpdatedAt = now
                },
                // Client2 booking deduction (Booking2)
                new Transaction
                {
                    Id = Txn5Id,
                    TransactionReference = "TXN20240901002",
                    WalletId = Wallet_CL2,
                    BookingId = Booking2Id,
                    Type = TransactionType.Booking,
                    Status = TransactionStatus.Success,
                    Amount = -60000m,
                    BalanceBefore = 60000m,
                    BalanceAfter = 0m,
                    Description = "Booking HAH-20240901-0002",
                    CreatedAt = now.AddDays(-6),
                    UpdatedAt = now
                },
                // Agency1 commission from Booking2
                new Transaction
                {
                    Id = Txn6Id,
                    TransactionReference = "TXN20240902001",
                    WalletId = Wallet_AA1,
                    BookingId = Booking2Id,
                    Type = TransactionType.Commission,
                    Status = TransactionStatus.Success,
                    Amount = 5100m,
                    BalanceBefore = 9900m,
                    BalanceAfter = 15000m,
                    Description = "Agency commission: HAH-20240901-0002",
                    CreatedAt = now.AddDays(-5),
                    UpdatedAt = now
                }
            );
            await db.SaveChangesAsync();
        }

        private static async Task SeedReviewsAsync(AppDbContext db)
        {
            if (await db.Reviews.AnyAsync()) return;

            var now = DateTime.UtcNow;
            db.Reviews.AddRange(
                // Client1 reviews Caregiver1 for Booking1 (5 stars)
                new Review
                {
                    Id = Review1Id,
                    BookingId = Booking1Id,
                    ReviewerUserId = Client1Id,
                    RevieweeUserId = Caregiver1Id,
                    Rating = 5,
                    IsByClient = true,
                    IsVisible = true,
                    Comment = "Amaka was absolutely wonderful. Very patient, caring, and professional. My mother loved her. Highly recommended!",
                    CreatedAt = now.AddMonths(-2).AddDays(1),
                    UpdatedAt = now
                },
                // Caregiver1 reviews Client1 for Booking1 (5 stars)
                new Review
                {
                    Id = Review2Id,
                    BookingId = Booking1Id,
                    ReviewerUserId = Caregiver1Id,
                    RevieweeUserId = Client1Id,
                    Rating = 5,
                    IsByClient = false,
                    IsVisible = true,
                    Comment = "Mama Ngozi and her family were very welcoming and respectful. A pleasure to serve them.",
                    CreatedAt = now.AddMonths(-2).AddDays(2),
                    UpdatedAt = now
                },
                // Client4 reviews Caregiver2 — disputed booking (2 stars)
                new Review
                {
                    Id = Review3Id,
                    BookingId = Booking4Id,
                    ReviewerUserId = Client4Id,
                    RevieweeUserId = Caregiver2Id,
                    Rating = 2,
                    IsByClient = true,
                    IsVisible = true,
                    Comment = "Caregiver did not complete all errands and returned change short. Very disappointed.",
                    IsFlagged = false,
                    CreatedAt = now.AddDays(-7),
                    UpdatedAt = now
                },
                // Caregiver2 reviews Client4 (3 stars)
                new Review
                {
                    Id = Review4Id,
                    BookingId = Booking4Id,
                    ReviewerUserId = Caregiver2Id,
                    RevieweeUserId = Client4Id,
                    Rating = 3,
                    IsByClient = false,
                    IsVisible = true,
                    Comment = "Client had unclear instructions and changed the task mid-way. Communication could be better.",
                    CreatedAt = now.AddDays(-6),
                    UpdatedAt = now
                }
            );
            await db.SaveChangesAsync();
        }

        private static async Task SeedNotificationsAsync(AppDbContext db)
        {
            if (await db.Notifications.AnyAsync()) return;

            var now = DateTime.UtcNow;
            db.Notifications.AddRange(
                new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = Caregiver1Id,
                    Title = "New Booking Request",
                    Body = "You have a new booking request: HAH-20240915-0003 from Mrs Ifeoma Obi.",
                    Type = "booking",
                    ReferenceId = Booking3Id.ToString(),
                    IsRead = true,
                    IsSentPush = true,
                    CreatedAt = now.AddDays(-1),
                    UpdatedAt = now
                },
                new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = Client3Id,
                    Title = "Booking Accepted",
                    Body = "Your booking HAH-20240915-0003 has been accepted. Caregiver will arrive on schedule.",
                    Type = "booking",
                    ReferenceId = Booking3Id.ToString(),
                    IsRead = false,
                    IsSentPush = true,
                    CreatedAt = now.AddHours(-6),
                    UpdatedAt = now
                },
                new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = Client1Id,
                    Title = "Wallet Funded",
                    Body = "Your wallet has been credited with ₦100,000.00. New balance: ₦100,000.00.",
                    Type = "payment",
                    ReferenceId = null,
                    IsRead = true,
                    IsSentPush = true,
                    IsSentEmail = true,
                    CreatedAt = now.AddMonths(-4),
                    UpdatedAt = now
                },
                new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = Admin1Id,
                    Title = "New Dispute Raised",
                    Body = "Dispute raised for booking HAH-20240910-0004. Ticket: TKT-20240910-0001.",
                    Type = "dispute",
                    ReferenceId = Ticket2Id.ToString(),
                    IsRead = false,
                    IsSentPush = true,
                    CreatedAt = now.AddDays(-7),
                    UpdatedAt = now
                },
                new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = Family1Id,
                    Title = "EMERGENCY ALERT",
                    Body = "Your family member Mama Ngozi Adesanya has triggered an emergency. Location: 14 Glover Road, Ikoyi.",
                    Type = "emergency",
                    ReferenceId = Alert1Id.ToString(),
                    IsRead = true,
                    IsSentPush = true,
                    IsSentSms = true,
                    CreatedAt = now.AddDays(-14),
                    UpdatedAt = now
                },
                new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = Caregiver1Id,
                    Title = "Badge Upgraded!",
                    Body = "Congratulations! You have earned the Elite badge. Keep up the excellent work!",
                    Type = "badge",
                    ReferenceId = null,
                    IsRead = false,
                    IsSentPush = true,
                    CreatedAt = now.AddDays(-2),
                    UpdatedAt = now
                }
            );
            await db.SaveChangesAsync();
        }

        private static async Task SeedSupportTicketsAsync(AppDbContext db)
        {
            if (await db.SupportTickets.AnyAsync()) return;

            var now = DateTime.UtcNow;
            db.SupportTickets.AddRange(
                // General enquiry — Open
                new SupportTicket
                {
                    Id = Ticket1Id,
                    TicketNumber = "TKT-20240905-0001",
                    RaisedByUserId = Client1Id,
                    Subject = "How do I change my caregiver preference?",
                    Description = "I would like to update my gender preference for caregivers. How can I do this?",
                    Status = TicketStatus.Open,
                    Priority = TicketPriority.Low,
                    IsDispute = false,
                    CreatedAt = now.AddDays(-5),
                    UpdatedAt = now
                },
                // Dispute for Booking4 — InProgress, assigned to Admin1
                new SupportTicket
                {
                    Id = Ticket2Id,
                    TicketNumber = "TKT-20240910-0001",
                    RaisedByUserId = Client4Id,
                    BookingId = Booking4Id,
                    AssignedToAdminId = Admin1Id,
                    Subject = "Dispute: Caregiver did not complete errands — HAH-20240910-0004",
                    Description = "The caregiver left early without finishing all the items on the errand list. Also, the change returned was short by ₦2,000.",
                    Status = TicketStatus.InProgress,
                    Priority = TicketPriority.High,
                    IsDispute = true,
                    CreatedAt = now.AddDays(-7),
                    UpdatedAt = now
                },
                // Payment issue — Resolved
                new SupportTicket
                {
                    Id = Ticket3Id,
                    TicketNumber = "TKT-20240815-0002",
                    RaisedByUserId = Caregiver1Id,
                    Subject = "Earnings not reflected in wallet",
                    Description = "Booking HAH-20240801-0001 was completed but earnings are not in my wallet yet.",
                    Status = TicketStatus.Resolved,
                    Priority = TicketPriority.Medium,
                    IsDispute = false,
                    AssignedToAdminId = Admin2Id,
                    ResolutionNote = "Earnings were in a 24-hour hold period. Released automatically by the system.",
                    ResolvedAt = now.AddMonths(-2).AddDays(3),
                    CreatedAt = now.AddMonths(-2).AddDays(1),
                    UpdatedAt = now
                },
                // New agency onboarding question — Open
                new SupportTicket
                {
                    Id = Ticket4Id,
                    TicketNumber = "TKT-20240918-0003",
                    RaisedByUserId = AgencyAdmin2Id,
                    Subject = "Verification documents submission guidance",
                    Description = "What documents do we need to submit for agency verification? Our registration certificate is being processed by CAC.",
                    Status = TicketStatus.Open,
                    Priority = TicketPriority.Medium,
                    IsDispute = false,
                    CreatedAt = now.AddDays(-3),
                    UpdatedAt = now
                }
            );
            await db.SaveChangesAsync();
        }

        private static async Task SeedTicketMessagesAsync(AppDbContext db)
        {
            if (await db.TicketMessages.AnyAsync()) return;

            var now = DateTime.UtcNow;
            db.TicketMessages.AddRange(
                // Message on Ticket2 (Dispute) from Client4
                new TicketMessage
                {
                    Id = Guid.NewGuid(),
                    TicketId = Ticket2Id,
                    SenderUserId = Client4Id,
                    Message = "I have attached the shopping list I gave him. You can see items 3, 5 and 6 were not purchased.",
                    IsInternal = false,
                    CreatedAt = now.AddDays(-6),
                    UpdatedAt = now
                },
                // Admin1 reply on Ticket2
                new TicketMessage
                {
                    Id = Guid.NewGuid(),
                    TicketId = Ticket2Id,
                    SenderUserId = Admin1Id,
                    Message = "Thank you for the details. We have contacted the caregiver and are awaiting their response. We will resolve this within 48 hours.",
                    IsInternal = false,
                    CreatedAt = now.AddDays(-6).AddHours(3),
                    UpdatedAt = now
                },
                // Internal admin note on Ticket2
                new TicketMessage
                {
                    Id = Guid.NewGuid(),
                    TicketId = Ticket2Id,
                    SenderUserId = Admin1Id,
                    Message = "Spoke with caregiver. He claims he completed all items but client refuses to acknowledge. Check CCTV possibility. Consider partial refund.",
                    IsInternal = true,
                    CreatedAt = now.AddDays(-5),
                    UpdatedAt = now
                },
                // Message on Ticket3 from Admin2
                new TicketMessage
                {
                    Id = Guid.NewGuid(),
                    TicketId = Ticket3Id,
                    SenderUserId = Admin2Id,
                    Message = "Your earnings were held for 24 hours as per our standard policy. They have now been automatically released to your wallet. Please check your balance.",
                    IsInternal = false,
                    CreatedAt = now.AddMonths(-2).AddDays(2),
                    UpdatedAt = now
                }
            );
            await db.SaveChangesAsync();
        }

        private static async Task SeedEmergencyAlertsAsync(AppDbContext db)
        {
            if (await db.EmergencyAlerts.AnyAsync()) return;

            var now = DateTime.UtcNow;
            db.EmergencyAlerts.AddRange(
                // Alert1 — Resolved (Client1, during active Booking1)
                new EmergencyAlert
                {
                    Id = Alert1Id,
                    ClientProfileId = ClientProfile1Id,
                    ActiveBookingId = Booking1Id,
                    Status = AlertStatus.Resolved,
                    Latitude = 6.4550m,
                    Longitude = 3.4280m,
                    LocationAddress = "14 Glover Road, Ikoyi, Lagos",
                    Message = "Having chest pains. Need help.",
                    NotifiedFamily = true,
                    NotifiedAdmin = true,
                    NotifiedCaregiver = true,
                    RespondedAt = now.AddDays(-14).AddMinutes(8),
                    RespondedByUserId = Admin1Id,
                    ResolutionNote = "Caregiver was on site and assisted. Client stabilised. Family notified.",
                    CreatedAt = now.AddDays(-14),
                    UpdatedAt = now
                },
                // Alert2 — FalseAlarm (Client2)
                new EmergencyAlert
                {
                    Id = Alert2Id,
                    ClientProfileId = ClientProfile2Id,
                    Status = AlertStatus.FalseAlarm,
                    Latitude = 6.5480m,
                    Longitude = 3.3590m,
                    LocationAddress = "22 Opebi Road, Gbagada, Lagos",
                    Message = null,
                    NotifiedFamily = true,
                    NotifiedAdmin = true,
                    NotifiedCaregiver = false,
                    RespondedAt = now.AddDays(-10).AddMinutes(5),
                    RespondedByUserId = Admin2Id,
                    ResolutionNote = "Client accidentally pressed the button. No emergency.",
                    CreatedAt = now.AddDays(-10),
                    UpdatedAt = now
                },
                // Alert3 — Active (Client3, no booking active)
                new EmergencyAlert
                {
                    Id = Alert3Id,
                    ClientProfileId = ClientProfile3Id,
                    Status = AlertStatus.Active,
                    Latitude = 6.4417m,
                    Longitude = 7.4950m,
                    LocationAddress = "7 Abakaliki Road, Enugu South",
                    Message = "Fell in the bathroom.",
                    NotifiedFamily = false,
                    NotifiedAdmin = true,
                    NotifiedCaregiver = false,
                    CreatedAt = now.AddMinutes(-30),
                    UpdatedAt = now
                },
                // Alert4 — Responded (Client4)
                new EmergencyAlert
                {
                    Id = Alert4Id,
                    ClientProfileId = ClientProfile4Id,
                    Status = AlertStatus.Responded,
                    Latitude = 9.0570m,
                    Longitude = 7.4890m,
                    LocationAddress = "Plot 5, Wuse Zone 4, Abuja",
                    Message = "Feeling very dizzy.",
                    NotifiedFamily = true,
                    NotifiedAdmin = true,
                    NotifiedCaregiver = false,
                    RespondedAt = now.AddDays(-2).AddMinutes(10),
                    RespondedByUserId = Admin1Id,
                    CreatedAt = now.AddDays(-2),
                    UpdatedAt = now
                }
            );
            await db.SaveChangesAsync();
        }

        private static async Task SeedOtpCodesAsync(AppDbContext db)
        {
            if (await db.OtpCodes.AnyAsync()) return;

            var now = DateTime.UtcNow;
            db.OtpCodes.AddRange(
                // Used email OTP for Client1 (historical)
                new OtpCode
                {
                    Id = Guid.NewGuid(),
                    UserId = Client1Id,
                    Code = "482931",
                    Purpose = "EmailVerify",
                    ExpiresAt = now.AddMonths(-5).AddMinutes(10),
                    IsUsed = true,
                    Attempts = 1,
                    CreatedAt = now.AddMonths(-5),
                    UpdatedAt = now
                },
                // Active phone OTP for Caregiver5 (pending verification)
                new OtpCode
                {
                    Id = Guid.NewGuid(),
                    UserId = Caregiver5Id,
                    Code = "719204",
                    Purpose = "PhoneVerify",
                    ExpiresAt = now.AddMinutes(8),
                    IsUsed = false,
                    Attempts = 0,
                    CreatedAt = now.AddMinutes(-2),
                    UpdatedAt = now
                },
                // Expired password reset for Client3
                new OtpCode
                {
                    Id = Guid.NewGuid(),
                    UserId = Client3Id,
                    Code = "334512",
                    Purpose = "PasswordReset",
                    ExpiresAt = now.AddDays(-1),
                    IsUsed = false,
                    Attempts = 2,
                    CreatedAt = now.AddDays(-1).AddMinutes(-14),
                    UpdatedAt = now
                },
                // Active email OTP for AgencyAdmin2 (just registered)
                new OtpCode
                {
                    Id = Guid.NewGuid(),
                    UserId = AgencyAdmin2Id,
                    Code = "905671",
                    Purpose = "EmailVerify",
                    ExpiresAt = now.AddMinutes(9),
                    IsUsed = false,
                    Attempts = 0,
                    CreatedAt = now.AddMinutes(-1),
                    UpdatedAt = now
                }
            );
            await db.SaveChangesAsync();
        }

        private static async Task SeedRefreshTokensAsync(AppDbContext db)
        {
            if (await db.RefreshTokens.AnyAsync()) return;

            var now = DateTime.UtcNow;
            db.RefreshTokens.AddRange(
                // SuperAdmin active session
                new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    UserId = SuperAdminId,
                    Token = "sa_refresh_ABCDEF123456_" + Guid.NewGuid().ToString("N"),
                    ExpiresAt = now.AddDays(29),
                    IsRevoked = false,
                    DeviceInfo = "Chrome 120 / Windows 11",
                    IpAddress = "102.89.23.150",
                    CreatedAt = now.AddDays(-1),
                    UpdatedAt = now
                },
                // Client1 active session (mobile)
                new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    UserId = Client1Id,
                    Token = "cl1_refresh_GHIJKL789012_" + Guid.NewGuid().ToString("N"),
                    ExpiresAt = now.AddDays(27),
                    IsRevoked = false,
                    DeviceInfo = "HelpAtHome Android App v1.2",
                    IpAddress = "105.112.50.200",
                    CreatedAt = now.AddDays(-3),
                    UpdatedAt = now
                },
                // Caregiver3 active session
                new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    UserId = Caregiver3Id,
                    Token = "cg3_refresh_MNOPQR345678_" + Guid.NewGuid().ToString("N"),
                    ExpiresAt = now.AddDays(25),
                    IsRevoked = false,
                    DeviceInfo = "HelpAtHome iOS App v1.2",
                    IpAddress = "197.210.85.63",
                    CreatedAt = now.AddDays(-5),
                    UpdatedAt = now
                },
                // Client2 revoked token (previous session)
                new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    UserId = Client2Id,
                    Token = "cl2_refresh_STUVWX901234_revoked",
                    ExpiresAt = now.AddDays(-2),
                    IsRevoked = true,
                    RevokedReason = "User logout",
                    DeviceInfo = "Firefox 119 / Ubuntu",
                    IpAddress = "105.112.44.100",
                    CreatedAt = now.AddDays(-32),
                    UpdatedAt = now
                }
            );
            await db.SaveChangesAsync();
        }

        private static async Task SeedAuditLogsAsync(MongoDbContext mongo, MongoDbSettings settings)
        {
            var col = mongo.GetCollection<AuditLog>(settings.AuditLogsCollection);
            if (await col.CountDocumentsAsync(FilterDefinition<AuditLog>.Empty) > 0) return;

            var now = DateTime.UtcNow;
            await col.InsertManyAsync(new[]
            {
            new AuditLog {
                PerformedByUserId = SuperAdminId.ToString(),
                PerformedByRole   = "SuperAdmin",
                Action            = AuditAction.Create,
                EntityName        = "User",
                EntityId          = Admin1Id.ToString(),
                NewValues         = "{\"email\":\"admin1@helpathoment.ng\",\"role\":\"Admin\"}",
                IpAddress         = "102.89.23.150",
                Notes             = "Admin account created during setup",
                CreatedAt         = now.AddMonths(-6)
            },
            new AuditLog {
                PerformedByUserId = Admin1Id.ToString(),
                PerformedByRole   = "Admin",
                Action            = AuditAction.Approve,
                EntityName        = "CaregiverProfile",
                EntityId          = CgProfile1Id.ToString(),
                Notes             = "Approved",
                IpAddress         = "102.90.10.22",
                CreatedAt         = now.AddMonths(-2)
            },
            new AuditLog {
                PerformedByUserId = Client1Id.ToString(),
                PerformedByRole   = "Client",
                Action            = AuditAction.Payment,
                EntityName        = "Wallet",
                EntityId          = Wallet_CL1.ToString(),
                Notes             = "Deposit ₦100,000 via Paystack ref pay_abc123def456",
                IpAddress         = "105.112.50.200",
                CreatedAt         = now.AddMonths(-4)
            },
            new AuditLog {
                PerformedByUserId = Admin1Id.ToString(),
                PerformedByRole   = "Admin",
                Action            = AuditAction.Approve,
                EntityName        = "Agency",
                EntityId          = Agency1Id.ToString(),
                Notes             = "Agency CareBridge Services verified after CAC doc review",
                IpAddress         = "102.90.10.22",
                CreatedAt         = now.AddMonths(-3)
            },
            new AuditLog {
                PerformedByUserId = Client4Id.ToString(),
                PerformedByRole   = "Client",
                Action            = AuditAction.Login,
                EntityName        = "User",
                EntityId          = Client4Id.ToString(),
                IpAddress         = "197.211.60.80",
                Notes             = "Login from mobile app",
                CreatedAt         = now.AddDays(-1)
            }
        });
        }

        private static async Task SeedAddressesAsync(AppDbContext db)
        {
            // Idempotency guard — covers all three tables
            if (await db.ClientAddresses.AnyAsync() ||
                await db.AgencyAddresses.AnyAsync() ||
                await db.CaregiverAddresses.AnyAsync())
                return;

            var now = DateTime.UtcNow;

            // ── CLIENT ADDRESSES ──────────────────────────────────────────
            db.ClientAddresses.AddRange(

                // Client 1 — Mama Ngozi Adesanya (Ikoyi, Lagos)
                new ClientAddress
                {
                    Id = ClAddr1Id,
                    ClientProfileId = ClientProfile1Id,
                    Line1 = "14 Glover Road",
                    Line2 = "Flat 3, Adeola Court",
                    Locality = "Ikoyi",
                    City = "Lagos",
                    LGA = "Eti-Osa",
                    State = "Lagos",
                    Country = "Nigeria",
                    PostalCode = "101233",
                    Latitude = 6.4550m,
                    Longitude = 3.4280m,
                    CreatedAt = now,
                    UpdatedAt = now
                },

                // Client 2 — Pa Adewale Sanni (Gbagada, Lagos)
                new ClientAddress
                {
                    Id = ClAddr2Id,
                    ClientProfileId = ClientProfile2Id,
                    Line1 = "22 Opebi Road",
                    Locality = "Gbagada",
                    City = "Lagos",
                    LGA = "Kosofe",
                    State = "Lagos",
                    Country = "Nigeria",
                    Latitude = 6.5480m,
                    Longitude = 3.3590m,
                    CreatedAt = now,
                    UpdatedAt = now
                },

                // Client 3 — Mrs Ifeoma Obi (Enugu South)
                new ClientAddress
                {
                    Id = ClAddr3Id,
                    ClientProfileId = ClientProfile3Id,
                    Line1 = "7 Abakaliki Road",
                    Locality = "Enugu",
                    City = "Enugu",
                    LGA = "Enugu South",
                    State = "Enugu",
                    Country = "Nigeria",
                    Latitude = 6.4417m,
                    Longitude = 7.4950m,
                    CreatedAt = now,
                    UpdatedAt = now
                },

                // Client 4 — Chief Bello Abdullahi (Wuse II, Abuja)
                new ClientAddress
                {
                    Id = ClAddr4Id,
                    ClientProfileId = ClientProfile4Id,
                    Line1 = "Plot 5, Wuse Zone 4",
                    Locality = "Wuse II",
                    City = "Abuja",
                    LGA = "Municipal Area Council",
                    State = "FCT",
                    Country = "Nigeria",
                    PostalCode = "900103",
                    Latitude = 9.0570m,
                    Longitude = 7.4890m,
                    CreatedAt = now,
                    UpdatedAt = now
                }
            );

            // ── AGENCY ADDRESSES ──────────────────────────────────────────
            db.AgencyAddresses.AddRange(

                // Agency 1 — CareBridge Services Ltd (Lekki, Lagos)
                new AgencyAddress
                {
                    Id = AgAddr1Id,
                    AgencyId = Agency1Id,
                    Line1 = "12 Admiralty Way",
                    Locality = "Lekki Phase 1",
                    City = "Lagos",
                    LGA = "Eti-Osa",
                    State = "Lagos",
                    Country = "Nigeria",
                    PostalCode = "106104",
                    CreatedAt = now,
                    UpdatedAt = now
                },

                // Agency 2 — HomeCare Enugu Ltd (Enugu)
                new AgencyAddress
                {
                    Id = AgAddr2Id,
                    AgencyId = Agency2Id,
                    Line1 = "5 Independence Layout",
                    Locality = "Independence Layout",
                    City = "Enugu",
                    LGA = "Enugu East",
                    State = "Enugu",
                    Country = "Nigeria",
                    CreatedAt = now,
                    UpdatedAt = now
                }
            );

            // ── CAREGIVER ADDRESSES ───────────────────────────────────────
            db.CaregiverAddresses.AddRange(

                // Caregiver 1 — Amaka Eze (Individual, Surulere)
                new CaregiverAddress
                {
                    Id = CgAddr1Id,
                    CaregiverProfileId = CgProfile1Id,
                    AgencyId = null,   // individual caregiver
                    Line1 = "45 Bode Thomas Street",
                    Locality = "Surulere",
                    City = "Lagos",
                    LGA = "Surulere",
                    State = "Lagos",
                    Country = "Nigeria",
                    CreatedAt = now,
                    UpdatedAt = now
                },

                // Caregiver 2 — Tunde Abiodun (Individual, Yaba)
                new CaregiverAddress
                {
                    Id = CgAddr2Id,
                    CaregiverProfileId = CgProfile2Id,
                    AgencyId = null,   // individual caregiver
                    Line1 = "18 Herbert Macaulay Way",
                    Locality = "Yaba",
                    City = "Lagos",
                    LGA = "Yaba",
                    State = "Lagos",
                    Country = "Nigeria",
                    CreatedAt = now,
                    UpdatedAt = now
                },

                // Caregiver 3 — Blessing Okonkwo (Agency 1, Lekki)
                new CaregiverAddress
                {
                    Id = CgAddr3Id,
                    CaregiverProfileId = CgProfile3Id,
                    AgencyId = Agency1Id,
                    Line1 = "7 Fola Osibo Road",
                    Locality = "Lekki Phase 1",
                    City = "Lagos",
                    LGA = "Eti-Osa",
                    State = "Lagos",
                    Country = "Nigeria",
                    CreatedAt = now,
                    UpdatedAt = now
                },

                // Caregiver 4 — Emeka Osei (Agency 1, Ajah)
                new CaregiverAddress
                {
                    Id = CgAddr4Id,
                    CaregiverProfileId = CgProfile4Id,
                    AgencyId = Agency1Id,
                    Line1 = "3 Orchid Hotel Road",
                    Locality = "Ajah",
                    City = "Lagos",
                    LGA = "Eti-Osa",
                    State = "Lagos",
                    Country = "Nigeria",
                    CreatedAt = now,
                    UpdatedAt = now
                },

                // Caregiver 5 — Chidinma Udo (Agency 1, Enugu)
                new CaregiverAddress
                {
                    Id = CgAddr5Id,
                    CaregiverProfileId = CgProfile5Id,
                    AgencyId = Agency1Id,
                    Line1 = "22 New Haven Road",
                    Locality = "New Haven",
                    City = "Enugu",
                    LGA = "Enugu North",
                    State = "Enugu",
                    Country = "Nigeria",
                    CreatedAt = now,
                    UpdatedAt = now
                }
            );

            await db.SaveChangesAsync();
        }


        private static async Task SeedAgencyActivityLogsAsync(
        MongoDbContext mongo, MongoDbSettings settings)
        {
            var col = mongo.GetCollection<AgencyActivityLog>(settings.AgencyActivityCollection);
            if (await col.CountDocumentsAsync(FilterDefinition<AgencyActivityLog>.Empty) > 0)
                return;

            var now = DateTime.UtcNow;
            await col.InsertManyAsync(new[]
            {
            new AgencyActivityLog {
                AgencyId          = Agency1Id.ToString(),
                AgencyAdminUserId = AgencyAdmin1Id.ToString(),
                Action            = "CaregiverAdded",
                CaregiverId       = CgProfile3Id.ToString(),
                Details           = "Blessing Okonkwo added to CareBridge Services Ltd",
                CreatedAt         = now.AddMonths(-10)
            },
            new AgencyActivityLog {
                AgencyId          = Agency1Id.ToString(),
                AgencyAdminUserId = AgencyAdmin1Id.ToString(),
                Action            = "CaregiverAdded",
                CaregiverId       = CgProfile4Id.ToString(),
                Details           = "Emeka Osei added to CareBridge Services Ltd",
                CreatedAt         = now.AddMonths(-1)
            },
            new AgencyActivityLog {
                AgencyId          = Agency1Id.ToString(),
                AgencyAdminUserId = AgencyAdmin1Id.ToString(),
                Action            = "BookingFulfilled",
                CaregiverId       = CgProfile3Id.ToString(),
                BookingId         = Booking2Id.ToString(),
                Details           = "Booking HAH-20240901-0002 in progress — Blessing Okonkwo",
                CreatedAt         = now.AddDays(-5)
            },
            new AgencyActivityLog {
                AgencyId          = Agency1Id.ToString(),
                AgencyAdminUserId = AgencyAdmin1Id.ToString(),
                Action            = "PayoutReceived",
                BookingId         = Booking2Id.ToString(),
                Details           = "Commission ₦5,100 received for booking HAH-20240901-0002",
                CreatedAt         = now.AddDays(-4)
            }
        });
        }

    }
}