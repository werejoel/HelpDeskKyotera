using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDeskKyotera.Migrations
{
    /// <inheritdoc />
    public partial class ddddv : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-1234-567890abcdef"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "UserName" },
                values: new object[] { "78fc467a-f3c8-4912-8afc-c246f0571aff", "AQAAAAIAAYagAAAAELMQ+mUtXLG9JApdOSBzeqRie6V7lEHFG7+7WMReV6lIwqCjCtDeyBn74aupUXB5Mg==", "admin@helpdsk.com" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-1234-567890abcdef"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "UserName" },
                values: new object[] { "86212189-c83e-4d9e-a08b-e8c5ba90643d", "AQAAAAIAAYagAAAAEPZbY6IG8bLCJjneEzhItdhP4tJ9J3hnJWnMcKx6Yj5GXiZXUEo9B/me+ZJgqoWMOA==", "admin@tms.com" });
        }
    }
}
