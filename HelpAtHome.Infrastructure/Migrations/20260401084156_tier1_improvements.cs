using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpAtHome.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class tier1_improvements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create composite replacements FIRST so MySQL will allow dropping the
            // old single-column FK-backing indexes in the next step.
            migrationBuilder.CreateIndex(
                name: "IX_CaregiverProfiles_AgencyId_VerificationStatus",
                table: "CaregiverProfiles",
                columns: new[] { "AgencyId", "VerificationStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CaregiverProfileId_Status_CreatedAt",
                table: "Bookings",
                columns: new[] { "CaregiverProfileId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ClientProfileId_Status_CreatedAt",
                table: "Bookings",
                columns: new[] { "ClientProfileId", "Status", "CreatedAt" });

            // Now safe to drop — MySQL sees the composite indexes above as valid
            // FK-backing indexes for AgencyId, CaregiverProfileId, ClientProfileId.
            migrationBuilder.DropIndex(
                name: "IX_CaregiverProfiles_AgencyId",
                table: "CaregiverProfiles");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_CaregiverProfileId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_ClientProfileId",
                table: "Bookings");

            migrationBuilder.AlterColumn<Guid>(
                name: "ReviewedByAdminId",
                table: "VerificationDocuments",
                type: "char(100)",
                maxLength: 100,
                nullable: true,
                collation: "ascii_general_ci",
                oldClrType: typeof(string),
                oldType: "char(100)",
                oldMaxLength: 100,
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<Guid>(
                name: "AssignedToAdminId",
                table: "SupportTickets",
                type: "char(100)",
                maxLength: 100,
                nullable: true,
                collation: "ascii_general_ci",
                oldClrType: typeof(string),
                oldType: "char(100)",
                oldMaxLength: 100,
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<decimal>(
                name: "WeeklyRate",
                table: "CaregiverProfiles",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");

            migrationBuilder.AlterColumn<string>(
                name: "SpecialInstructions",
                table: "Bookings",
                type: "varchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<decimal>(
                name: "ClientLongitude",
                table: "Bookings",
                type: "decimal(10,7)",
                precision: 10,
                scale: 7,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ClientLatitude",
                table: "Bookings",
                type: "decimal(10,7)",
                precision: 10,
                scale: 7,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ClientAddress",
                table: "Bookings",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "CancelledBy",
                table: "Bookings",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "CancellationReason",
                table: "Bookings",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_WalletId_CreatedAt",
                table: "Transactions",
                columns: new[] { "WalletId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_AssignedToAdminId_Status",
                table: "SupportTickets",
                columns: new[] { "AssignedToAdminId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_FamilyAccesses_ClientUserId_IsApproved_IsDeleted",
                table: "FamilyAccesses",
                columns: new[] { "ClientUserId", "IsApproved", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_FamilyAccesses_IsDeleted",
                table: "FamilyAccesses",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_CaregiverProfiles_IsDeleted",
                table: "CaregiverProfiles",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CaregiverProfileId_Status_ScheduledStartDate",
                table: "Bookings",
                columns: new[] { "CaregiverProfileId", "Status", "ScheduledStartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_IsDeleted",
                table: "Bookings",
                column: "IsDeleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_WalletId_CreatedAt",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_SupportTickets_AssignedToAdminId_Status",
                table: "SupportTickets");

            migrationBuilder.DropIndex(
                name: "IX_FamilyAccesses_ClientUserId_IsApproved_IsDeleted",
                table: "FamilyAccesses");

            migrationBuilder.DropIndex(
                name: "IX_FamilyAccesses_IsDeleted",
                table: "FamilyAccesses");

            migrationBuilder.DropIndex(
                name: "IX_CaregiverProfiles_AgencyId_VerificationStatus",
                table: "CaregiverProfiles");

            migrationBuilder.DropIndex(
                name: "IX_CaregiverProfiles_IsDeleted",
                table: "CaregiverProfiles");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_CaregiverProfileId_Status_CreatedAt",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_CaregiverProfileId_Status_ScheduledStartDate",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_ClientProfileId_Status_CreatedAt",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_IsDeleted",
                table: "Bookings");

            migrationBuilder.AlterColumn<string>(
                name: "ReviewedByAdminId",
                table: "VerificationDocuments",
                type: "char(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "char(100)",
                oldMaxLength: 100,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<string>(
                name: "AssignedToAdminId",
                table: "SupportTickets",
                type: "char(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "char(100)",
                oldMaxLength: 100,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<decimal>(
                name: "WeeklyRate",
                table: "CaregiverProfiles",
                type: "decimal(65,30)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "SpecialInstructions",
                table: "Bookings",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<decimal>(
                name: "ClientLongitude",
                table: "Bookings",
                type: "decimal(65,30)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,7)",
                oldPrecision: 10,
                oldScale: 7,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ClientLatitude",
                table: "Bookings",
                type: "decimal(65,30)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,7)",
                oldPrecision: 10,
                oldScale: 7,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ClientAddress",
                table: "Bookings",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "CancelledBy",
                table: "Bookings",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "CancellationReason",
                table: "Bookings",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CaregiverProfiles_AgencyId",
                table: "CaregiverProfiles",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CaregiverProfileId",
                table: "Bookings",
                column: "CaregiverProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ClientProfileId",
                table: "Bookings",
                column: "ClientProfileId");
        }
    }
}
