using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDeskKyotera.Migrations
{
    /// <inheritdoc />
    public partial class ddddvV : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-1234-567890abcdef"),
                columns: new[] { "ConcurrencyStamp", "Email", "NormalizedEmail", "NormalizedUserName", "PasswordHash" },
                values: new object[] { "9828cfa7-6879-4c30-9e70-4b82a9f5b1c4", "admin@helpdsk.com", "ADMIN@HELPDSK.COM", "ADMIN@HELPDSK.COM", "AQAAAAIAAYagAAAAEGRb7EY1haGe0NGdNTIJCU6w/hHmvjwu38RLrALxlWJCW7TN2awksUNq6QtcbPULig==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-1234-567890abcdef"),
                columns: new[] { "ConcurrencyStamp", "Email", "NormalizedEmail", "NormalizedUserName", "PasswordHash" },
                values: new object[] { "78fc467a-f3c8-4912-8afc-c246f0571aff", "admin@tms.com", "ADMIN@TMS.COM", "ADMIN@TMS.COM", "AQAAAAIAAYagAAAAELMQ+mUtXLG9JApdOSBzeqRie6V7lEHFG7+7WMReV6lIwqCjCtDeyBn74aupUXB5Mg==" });
        }
    }
}
