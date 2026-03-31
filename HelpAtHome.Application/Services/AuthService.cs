using AutoMapper;
using HelpAtHome.Application.Interfaces;
using HelpAtHome.Application.Interfaces.Services;
using HelpAtHome.Core.DTOs.Requests.Auth;
using HelpAtHome.Core.DTOs.Responses.Auth;
using HelpAtHome.Core.Entities;
using HelpAtHome.Core.Enums;
using HelpAtHome.Shared;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;

namespace HelpAtHome.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<User> _userManager;       // Identity
        private readonly SignInManager<User> _signInManager;   // Identity
        private readonly IJwtService _jwtService;
        private readonly INotificationService _notification;
        //private readonly IAuditLogService _audit;
        private readonly IMapper _mapper;

        public AuthService(IUnitOfWork uow, UserManager<User> userManager, SignInManager<User> signInManager, 
            IJwtService jwtService, INotificationService notification, //IAuditLogService audit, 
            IMapper mapper)
        {
            _uow = uow;
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _notification = notification;
            //_audit = audit;
            _mapper = mapper;
        }

        public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto, string ip)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || user.IsDeleted)
                return Result<AuthResponseDto>.Fail("Invalid credentials");
            if (user.IsSuspended)
                return Result<AuthResponseDto>.Fail("Account suspended: " + user.SuspensionReason);

            // SignInManager checks password AND enforces Identity lockout policy.
            // Replaces manual BCrypt.Verify + FailedLoginAttempts + LockoutUntil logic.
            var signIn = await _signInManager.CheckPasswordSignInAsync(
                user, dto.Password, lockoutOnFailure: true);

            if (signIn.IsLockedOut)
                return Result<AuthResponseDto>.Fail(
                    $"Account locked until {user.LockoutEnd?.UtcDateTime:HH:mm UTC}.");
            if (!signIn.Succeeded)
                return Result<AuthResponseDto>.Fail("Invalid credentials");

            user.LastLoginAt = DateTime.UtcNow;
            user.LastLoginIp = ip;
            await _userManager.UpdateAsync(user);

            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);
            var accessToken = _jwtService.GenerateAccessToken(user, roles, claims);
            var refreshToken = _jwtService.GenerateRefreshToken();

            await _uow.RefreshTokens.AddAsync(new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                IpAddress = ip
            });
            await _uow.SaveChangesAsync();
            //await _audit.LogAsync(user.Id.ToString(), user.Role.ToString(), AuditAction.Login, "User", user.Id.ToString(), ipAddress: ip);

            return Result<AuthResponseDto>.Ok(new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                UserId = user.Id,
                Role = user.Role,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                FullName = user.FullName,
                Email = user.Email
            });
        }

        public async Task<Result> ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return Result.Ok();
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _notification.SendPasswordResetEmailAsync(user.Email!, token, user.FullName);
            return Result.Ok();
        }

        public async Task<Result> ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return Result.Fail("User not found");
            var result = await _userManager.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword);
            return result.Succeeded ? Result.Ok()
                : Result.Fail(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        public async Task<Result<AuthResponseDto>> RegisterClientAsync(RegisterClientDto dto)
        {
            if (await _userManager.FindByEmailAsync(dto.Email) != null)
                return Result<AuthResponseDto>.Fail("Email is already registered.");
            if ((await _uow.Users.GetByPhoneAsync(dto.PhoneNumber)) != null)
                return Result<AuthResponseDto>.Fail("Phone number is already registered.");

            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhoneNumber = dto.PhoneNumber,
                Role = UserRole.Client,
                IsActive = true,
                LockoutEnabled = true
            };
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return Result<AuthResponseDto>.Fail(
                    string.Join(", ", result.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(user, nameof(UserRole.Client));

            var clientProfileId = Guid.NewGuid(); // Generate a new Guid for the ClientProfile
            await _uow.ClientProfiles.AddAsync(new ClientProfile
            {
                Id = clientProfileId, // Use the generated Guid as the Id for ClientProfile
                UserId = user.Id,
                DateOfBirth = dto.DateOfBirth,
                EmergencyContactName = dto.EmergencyContactName,
                EmergencyContactPhoneNumber = dto.EmergencyContactPhoneNumber,
                Gender = dto.Gender,
                MedicalConditions = dto.MedicalConditions,
                SpecialNotes = dto.SpecialNotes,
                RelationToRecipient = dto.RelationToRecipient,
                ServicesNeeded = dto.ServicesNeeded,
                Frequency = dto.Frequency,
                CareGiverGenderPreference = dto.CareGiverGenderPreference,
                RequireVerifiedOnly = dto.RequireVerifiedOnly,
                Address = new ClientAddress
                {
                    Id = Guid.NewGuid(),
                    ClientProfileId = clientProfileId,
                    Line1 = dto.Address.Line1,
                    Line2 = dto.Address.Line2,
                    Locality = dto.Address.Locality,
                    City = dto.Address.City,
                    LGA = dto.Address.LGA,
                    State = dto.Address.State,
                    Country = dto.Address.Country,
                    PostalCode = dto.Address.PostalCode,
                    Latitude = dto.Address.Latitude,
                    Longitude = dto.Address.Longitude
                }
            });

            // create wallet for client
            await _uow.Wallets.AddAsync(new Wallet { 
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Balance = 0,
                User = user
            });
            await _uow.SaveChangesAsync();

            await SendEmailOtpAsync(user.Id);
            //await _audit.LogAsync(user.Id.ToString(), user.Role.ToString(), AuditAction.Create, "User", user.Id.ToString());
            return Result<AuthResponseDto>.Ok(await BuildAuthResponseAsync(user));

        }

        public async Task<Result<AuthResponseDto>> RegisterIndividualCaregiverAsync(RegisterCaregiverDto dto)
        {
            // Run both uniqueness checks in parallel
            var emailTask = _userManager.FindByEmailAsync(dto.Email);
            var phoneTask = _uow.Users.GetByPhoneAsync(dto.PhoneNumber);
            await Task.WhenAll(emailTask, phoneTask);

            if (emailTask.Result != null)
                return Result<AuthResponseDto>.Fail("Email is already registered.");
            if (phoneTask.Result != null)
                return Result<AuthResponseDto>.Fail("Phone number is already registered.");

            var user = BuildCaregiverUser(dto);

            await _uow.BeginTransactionAsync();
            try
            {
                var identityResult = await _userManager.CreateAsync(user, dto.Password);
                if (!identityResult.Succeeded)
                {
                    await _uow.RollbackAsync();
                    return Result<AuthResponseDto>.Fail(
                        string.Join(", ", identityResult.Errors.Select(e => e.Description)));
                }

                await _userManager.AddToRoleAsync(user, nameof(UserRole.IndividualCaregiver));

                var profile = BuildCaregiverProfile(dto, user);
                await _uow.CaregiverProfiles.AddAsync(profile);
                await _uow.Wallets.AddAsync(new Wallet { Id = Guid.NewGuid(), UserId = user.Id });

                if (dto.ServiceCategoryIds?.Count > 0)
                {
                    foreach (var catId in dto.ServiceCategoryIds)
                        await _uow.CaregiverServices.AddAsync(new CaregiverService
                        {
                            Id = Guid.NewGuid(),
                            CaregiverProfileId = profile.Id,
                            ServiceCategoryId = catId
                        });
                }

                await _uow.SaveChangesAsync();
                await _uow.CommitAsync();
            }
            catch(Exception ex)
            {
                await _uow.RollbackAsync();
                return Result<AuthResponseDto>.Fail($"Registration failed. Please try again. {ex.Message}");
            }

            await SendEmailOtpAsync(user.Id);
            //await _audit.LogAsync(user.Id.ToString(), user.Role.ToString(), AuditAction.Create, "User", user.Id.ToString());
            return Result<AuthResponseDto>.Ok(await BuildAuthResponseAsync(user));
        }

        public async Task<Result<AuthResponseDto>> RegisterAgencyAdminAsync(RegisterAgencyAdminDto dto)
        {
            if (await _userManager.FindByEmailAsync(dto.Email) != null)
                return Result<AuthResponseDto>.Fail("Email is already registered.");
            if ((await _uow.Users.GetByPhoneAsync(dto.PhoneNumber)) != null)
                return Result<AuthResponseDto>.Fail("Phone number is already registered.");
            if (await _uow.Agencies.RegistrationNumberExistsAsync(dto.RegistrationNumber))
                return Result<AuthResponseDto>.Fail("Agency registration number already exists.");

            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhoneNumber = dto.PhoneNumber,
                Role = UserRole.AgencyAdmin,
                IsActive = true,
                LockoutEnabled = true
            };
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return Result<AuthResponseDto>.Fail(
                    string.Join(", ", result.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(user, nameof(UserRole.AgencyAdmin));

            var agencyId = Guid.NewGuid(); // Generate a new Guid for the Agency
            var agency = new Agency
            {
                Id = agencyId,
                AgencyAdminUserId = user.Id,
                AgencyName = dto.AgencyName,
                RegistrationNumber = dto.RegistrationNumber,
                Email = dto.AgencyEmail,
                PhoneNumber = dto.AgencyPhone,
                AgencyAddress = new AgencyAddress() {
                    Id = Guid.NewGuid(),
                    AgencyId = agencyId,
                    Line1 = dto.AgencyAddress.Line1,
                    Line2 = dto.AgencyAddress.Line2,
                    Locality = dto.AgencyAddress.Locality,
                    City = dto.AgencyAddress.City,
                    LGA = dto.AgencyAddress.LGA,
                    State = dto.AgencyAddress.State,
                    Country = dto.AgencyAddress.Country,
                    PostalCode = dto.AgencyAddress.PostalCode
                },
                Description = dto.AgencyDescription,
                Website = dto.Website,
                VerificationStatus = VerificationStatus.Pending,
                IsActive = true
            };

            await _uow.Agencies.AddAsync(agency);
            await _uow.Wallets.AddAsync(new Wallet { 
                Id = Guid.NewGuid(), 
                UserId = user.Id 
            });
            await _uow.SaveChangesAsync();

            await SendEmailOtpAsync(user.Id);
            //await _audit.LogAsync(user.Id.ToString(), user.Role.ToString(), AuditAction.Create, "Agency", agency.Id.ToString());
            return Result<AuthResponseDto>.Ok(await BuildAuthResponseAsync(user));

        }

        // Called by AgencyService.AddCaregiverAsync — not directly by a controller.
        // Exposed on IAuthService so AgencyService can reuse the token flow.
        public async Task<Result<Guid>> RegisterAgencyCaregiverAsync(RegisterAgencyCaregiverDto dto, Guid agencyId)
        {
            if (await _userManager.FindByEmailAsync(dto.Email) != null)
                return Result<Guid>.Fail("Email already registered");
            if (await _uow.Users.PhoneExistsAsync(dto.PhoneNumber))
                return Result<Guid>.Fail("Phone number already registered");

            var agency = await _uow.Agencies.GetByIdAsync(agencyId);
            if (agency == null)
                return Result<Guid>.Fail("Agency not found");
            if (agency.VerificationStatus != VerificationStatus.Approved)
                return Result<Guid>.Fail("Agency not yet verified");

            if (dto.ServiceCategoryIds.Count > 0)
            {
                var validCount = await _uow.ServiceCategories.CountAsync(
                    sc => dto.ServiceCategoryIds.Contains(sc.Id));
                if (validCount != dto.ServiceCategoryIds.Count)
                    return Result<Guid>.Fail("One or more service categories are invalid");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                PhoneNumber = dto.PhoneNumber,
                Role = UserRole.AgencyCaregiver,
                IsActive = true,
                LockoutEnabled = true
            };

            await _uow.BeginTransactionAsync();
            try
            {
                var result = await _userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                {
                    await _uow.RollbackAsync();
                    return Result<Guid>.Fail(string.Join("; ", result.Errors.Select(e => e.Description)));
                }

                await _userManager.AddToRoleAsync(user, nameof(UserRole.AgencyCaregiver));

                var caregiverProfileId = Guid.NewGuid();
                var profile = new CaregiverProfile
                {
                    Id = caregiverProfileId,
                    UserId = user.Id,
                    AgencyId = agencyId,
                    Bio = dto.Bio,
                    YearsOfExperience = dto.YearsOfExperience,
                    HourlyRate = dto.HourlyRate,
                    DailyRate = dto.DailyRate,
                    MonthlyRate = dto.MonthlyRate,
                    GenderProvided = dto.Gender,
                    VerificationStatus = VerificationStatus.Pending,
                    IsAvailable = true,
                    Address = new CaregiverAddress
                    {
                        Id = Guid.NewGuid(),
                        CaregiverProfileId = caregiverProfileId,
                        Line1 = dto.Address.Line1,
                        Line2 = dto.Address.Line2,
                        Locality = dto.Address.Locality,
                        City = dto.Address.City,
                        LGA = dto.Address.LGA,
                        State = dto.Address.State,
                        Country = dto.Address.Country,
                        PostalCode = dto.Address.PostalCode
                    }
                };
                await _uow.CaregiverProfiles.AddAsync(profile);
                await _uow.Wallets.AddAsync(new Wallet { UserId = user.Id });

                foreach (var catId in dto.ServiceCategoryIds)
                    await _uow.CaregiverServices.AddAsync(new CaregiverService
                    { CaregiverProfileId = profile.Id, ServiceCategoryId = catId });

                await _uow.SaveChangesAsync();
                await _uow.CommitAsync();
            }
            catch
            {
                await _uow.RollbackAsync();
                throw;
            }

            await _notification.SendAsync(user.Id, "Welcome to Help At Home", $"You have been added to {agency.AgencyName}.", "system", null);

            return Result<Guid>.Ok(user.Id);
        }

        public async Task<Result<AuthResponseDto>> RefreshTokenAsync(string refreshToken, string ipAddress)
        {
            var tokenRecord = await _uow.RefreshTokens.FirstOrDefaultAsync(
                t => t.Token == refreshToken && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow);

            if (tokenRecord == null)
                return Result<AuthResponseDto>.Fail("Invalid or expired refresh token.");

            var user = await _userManager.FindByIdAsync(tokenRecord.UserId.ToString());
            if (user == null || user.IsDeleted)
                return Result<AuthResponseDto>.Fail("User not found.");
            if (user.IsSuspended)
                return Result<AuthResponseDto>.Fail("Account suspended: " + user.SuspensionReason);

            // Rotate: revoke old token and issue a new pair
            tokenRecord.IsRevoked = true;
            tokenRecord.RevokedReason = "Rotated";
            _uow.RefreshTokens.Update(tokenRecord);

            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);
            var newAccessToken = _jwtService.GenerateAccessToken(user, roles, claims);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            await _uow.RefreshTokens.AddAsync(new RefreshToken
            {
                UserId = user.Id,
                Token = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                IpAddress = ipAddress
            });
            await _uow.SaveChangesAsync();

            return Result<AuthResponseDto>.Ok(new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                UserId = user.Id,
                Role = user.Role,
                RoleName = user.Role.ToString(),
                FullName = user.FullName,
                Email = user.Email!,
                IsEmailConfirmed = user.EmailConfirmed,
                IsPhoneConfirmed = user.PhoneNumberConfirmed,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            });
        }

        public async Task<Result> LogoutAsync(Guid userId, string refreshToken)
        {
            var tokenRecord = await _uow.RefreshTokens.FirstOrDefaultAsync(
                t => t.Token == refreshToken && t.UserId == userId && !t.IsRevoked);

            if (tokenRecord == null)
                return Result.Ok(); // Already revoked or not found — idempotent

            tokenRecord.IsRevoked = true;
            tokenRecord.RevokedReason = "Logout";
            _uow.RefreshTokens.Update(tokenRecord);
            await _uow.SaveChangesAsync();

            return Result.Ok();
        }

        public async Task<Result> SendEmailOtpAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return Result.Fail("User not found.");

            // Invalidate any existing active email OTPs for this user
            var existing = await _uow.OtpCodes.FindAsync(
                o => o.UserId == userId && o.Purpose == "EmailVerify" && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow);
            foreach (var old in existing)
            {
                old.IsUsed = true;
                _uow.OtpCodes.Update(old);
            }

            var code = GenerateOtp();
            await _uow.OtpCodes.AddAsync(new OtpCode
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Code = code,
                Purpose = "EmailVerify",
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            });
            await _uow.SaveChangesAsync();

            var html = $"<p>Hi {user.FullName},</p>" +
                       $"<p>Your email verification code is: <strong>{code}</strong></p>" +
                       "<p>This code expires in 10 minutes. Do not share it with anyone.</p>";
            await _notification.SendEmailAsync(user.Email!, "Verify Your Help At Home Account", html);

            return Result.Ok();
        }

        public async Task<Result> VerifyEmailOtpAsync(Guid userId, string otp)
        {
            var otpRecord = await _uow.OtpCodes.FirstOrDefaultAsync(
                o => o.UserId == userId && o.Purpose == "EmailVerify" && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow);

            if (otpRecord == null)
                return Result.Fail("Invalid or expired OTP. Please request a new code.");

            otpRecord.Attempts++;

            if (otpRecord.Attempts >= 5)
            {
                otpRecord.IsUsed = true;
                _uow.OtpCodes.Update(otpRecord);
                await _uow.SaveChangesAsync();
                return Result.Fail("Too many failed attempts. Please request a new code.");
            }

            if (otpRecord.Code != otp)
            {
                _uow.OtpCodes.Update(otpRecord);
                await _uow.SaveChangesAsync();
                return Result.Fail("Incorrect code.");
            }

            otpRecord.IsUsed = true;
            _uow.OtpCodes.Update(otpRecord);

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user != null)
            {
                user.EmailConfirmed = true;
                user.IsActive = true;   // activate account now that email is confirmed
                await _userManager.UpdateAsync(user);
            }

            await _uow.SaveChangesAsync();
            return Result.Ok();
        }

        public async Task<Result> SendPhoneOtpAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return Result.Fail("User not found.");
            if (string.IsNullOrEmpty(user.PhoneNumber))
                return Result.Fail("No phone number associated with this account.");

            // Invalidate any existing active phone OTPs for this user
            var existing = await _uow.OtpCodes.FindAsync(
                o => o.UserId == userId && o.Purpose == "PhoneVerify" && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow);
            foreach (var old in existing)
            {
                old.IsUsed = true;
                _uow.OtpCodes.Update(old);
            }

            var code = GenerateOtp();
            await _uow.OtpCodes.AddAsync(new OtpCode
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Code = code,
                Purpose = "PhoneVerify",
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            });
            await _uow.SaveChangesAsync();

            await _notification.SendSmsAsync(user.PhoneNumber,
                $"Your Help At Home verification code is: {code}. It expires in 10 minutes.");

            return Result.Ok();
        }

        public async Task<Result> VerifyPhoneOtpAsync(Guid userId, string otp)
        {
            var otpRecord = await _uow.OtpCodes.FirstOrDefaultAsync(
                o => o.UserId == userId && o.Purpose == "PhoneVerify" && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow);

            if (otpRecord == null)
                return Result.Fail("Invalid or expired OTP. Please request a new code.");

            otpRecord.Attempts++;

            if (otpRecord.Attempts >= 5)
            {
                otpRecord.IsUsed = true;
                _uow.OtpCodes.Update(otpRecord);
                await _uow.SaveChangesAsync();
                return Result.Fail("Too many failed attempts. Please request a new code.");
            }

            if (otpRecord.Code != otp)
            {
                _uow.OtpCodes.Update(otpRecord);
                await _uow.SaveChangesAsync();
                return Result.Fail("Incorrect code.");
            }

            otpRecord.IsUsed = true;
            _uow.OtpCodes.Update(otpRecord);

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user != null)
            {
                user.PhoneNumberConfirmed = true;
                await _userManager.UpdateAsync(user);
            }

            await _uow.SaveChangesAsync();
            return Result.Ok();
        }

        public async Task<Result> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                return Result.Fail("User not found");
            }

            var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
            if (result.Succeeded)
                return Result.Ok();
            return Result.Fail("Failed to reset password");
        }

        // ── Private helpers ───────────────────────────────────────────────

        private static User BuildCaregiverUser(RegisterCaregiverDto dto) => new User
        {
            Id = Guid.NewGuid(),
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            PhoneNumber = dto.PhoneNumber,
            Role = UserRole.IndividualCaregiver,
            ProfilePhotoUrl = dto.ProfilePicture,
            IsActive = false,       // activated after email OTP is verified
            LockoutEnabled = true
        };

        private static CaregiverProfile BuildCaregiverProfile(RegisterCaregiverDto dto, User user)
        {
            var profileId = Guid.NewGuid();
            return new CaregiverProfile
            {
                Id = profileId,
                UserId = user.Id,
                Bio = dto.Bio,
                YearsOfExperience = dto.YearsOfExperience,
                HourlyRate = dto.HourlyRate,
                DailyRate = dto.DailyRate,
                WeeklyRate = dto.WeeklyRate,
                MonthlyRate = dto.MonthlyRate,
                GenderProvided = dto.Gender,
                VerificationStatus = VerificationStatus.Pending,
                Badge = BadgeLevel.New,
                Services = dto.ServicesToOffer,
                LanguagesSpoken = dto.LanguagesSpoken?.Count > 0
                    ? JsonSerializer.Serialize(dto.LanguagesSpoken)
                    : "[]",
                IdType = dto.IdType,
                IdNumber = dto.IdNumber,
                NextOfKinName = dto.NextOfKinName,
                NextOfKinPhoneNumber = dto.NextOfKinPhoneNumber,
                Address = BuildCaregiverAddress(dto, profileId)
            };
        }

        private static CaregiverAddress BuildCaregiverAddress(RegisterCaregiverDto dto, Guid profileId) =>
            new CaregiverAddress
            {
                Id = Guid.NewGuid(),
                CaregiverProfileId = profileId,
                AgencyId = null,    // individual — no agency
                Line1 = dto.Address.Line1,
                Line2 = dto.Address.Line2,
                Locality = dto.Address.Locality,
                City = dto.Address.City,
                LGA = dto.Address.LGA,
                State = dto.Address.State,
                Country = dto.Address.Country,
                PostalCode = dto.Address.PostalCode
            };

        private async Task<AuthResponseDto> BuildAuthResponseAsync(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);
            var accessToken = _jwtService.GenerateAccessToken(user, roles, claims);
            var refreshToken = _jwtService.GenerateRefreshToken();
            await _uow.RefreshTokens.AddAsync(new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            });
            await _uow.SaveChangesAsync();
            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                UserId = user.Id,
                Role = user.Role,
                RoleName = user.Role.ToString(),
                FullName = user.FullName,
                Email = user.Email!,
                IsEmailConfirmed = user.EmailConfirmed,
                IsPhoneConfirmed = user.PhoneNumberConfirmed,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };
        }

        // 6-digit numeric OTP
        private static string GenerateOtp()
        {
            return Random.Shared.Next(100_000, 999_999).ToString();
        }
    }





    /*public class AuthService2 : IAuthService
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IJwtService _jwtService;
        private readonly INotificationService _notification;
        private readonly IAuditLogService _audit;
        private readonly IMapper _mapper;

        public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto, string ip)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || user.IsDeleted)
                return Result<AuthResponseDto>.Fail("Invalid credentials");
            if (user.IsSuspended)
                return Result<AuthResponseDto>.Fail("Account suspended: " + user.SuspensionReason);
            var signIn = await _signInManager.CheckPasswordSignInAsync(
                user, dto.Password, lockoutOnFailure: true);
            if (signIn.IsLockedOut)
                return Result<AuthResponseDto>.Fail($"Account locked until {user.LockoutEnd?.UtcDateTime:HH:mm UTC}.");
            if (!signIn.Succeeded)
                return Result<AuthResponseDto>.Fail("Invalid credentials");
            user.LastLoginAt = DateTime.UtcNow; user.LastLoginIp = ip;
            await _userManager.UpdateAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);
            var accessToken = _jwtService.GenerateAccessToken(user, roles, claims);
            var refreshToken = _jwtService.GenerateRefreshToken();
            await _uow.RefreshTokens.AddAsync(new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                IpAddress = ip
            });
            await _uow.SaveChangesAsync();
            await _audit.LogAsync(user.Id.ToString(), user.Role.ToString(),
                AuditAction.Login, "User", user.Id.ToString(), ipAddress: ip);
            return Result<AuthResponseDto>.Ok(new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                UserId = user.Id,
                Role = user.Role,
                FullName = user.FullName,
                Email = user.Email!,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            });
        }

        public async Task<Result> ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return Result.Ok();
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _notification.SendPasswordResetEmailAsync(user.Email!, token, user.FullName);
            return Result.Ok();
        }

        public async Task<Result> ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return Result.Fail("User not found");
            var result = await _userManager.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword);
            return result.Succeeded ? Result.Ok()
                : Result.Fail(string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }*/

}
