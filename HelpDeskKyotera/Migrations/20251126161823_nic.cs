using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDeskKyotera.Migrations
{
    /// <inheritdoc />
    public partial class nic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedOn", "Email", "EmailConfirmed", "FirstName", "IsActive", "LastLogin", "LastName", "LockoutEnabled", "LockoutEnd", "ModifiedOn", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { new Guid("a1b2c3d4-e5f6-7890-1234-567890abcdef"), 0, "987de748-f182-488c-b882-f23194ccbc1b", new DateTime(2025, 11, 26, 16, 18, 21, 818, DateTimeKind.Utc).AddTicks(2817), "admin@tms.com", true, "System", true, null, "Administrator", false, null, new DateTime(2025, 11, 26, 16, 18, 21, 818, DateTimeKind.Utc).AddTicks(3276), "ADMIN@TMS.COM", "ADMIN@TMS.COM", "AQAAAAIAAYagAAAAEFbu5uuKBmrCwCleSZcmQ/MrYYIoOCdfx8MdulQR9oQDMmQcyOAq9BRriYL4XSKzRA==", "+256700000000", false, "0ddaa9ac-e1b1-4124-ad6f-926bd9c64cad", false, "admin@tms.com" });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { new Guid("c8d89a25-4b96-4f20-9d79-7f8a54c5213d"), new Guid("a1b2c3d4-e5f6-7890-1234-567890abcdef") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("c8d89a25-4b96-4f20-9d79-7f8a54c5213d"), new Guid("a1b2c3d4-e5f6-7890-1234-567890abcdef") });

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-1234-567890abcdef"));
        }
    }
}
