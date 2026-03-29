using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpAtHome.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class moreCareGiverFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AddColumn<string>(
                name: "DocumentPhotoUrl",
                table: "CaregiverProfiles",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "IdNumber",
                table: "CaregiverProfiles",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "IdType",
                table: "CaregiverProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "NextOfKinName",
                table: "CaregiverProfiles",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "NextOfKinPhoneNumber",
                table: "CaregiverProfiles",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentPhotoUrl",
                table: "CaregiverProfiles");

            migrationBuilder.DropColumn(
                name: "IdNumber",
                table: "CaregiverProfiles");

            migrationBuilder.DropColumn(
                name: "IdType",
                table: "CaregiverProfiles");

            migrationBuilder.DropColumn(
                name: "NextOfKinName",
                table: "CaregiverProfiles");

            migrationBuilder.DropColumn(
                name: "NextOfKinPhoneNumber",
                table: "CaregiverProfiles");

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
        }
    }
}
