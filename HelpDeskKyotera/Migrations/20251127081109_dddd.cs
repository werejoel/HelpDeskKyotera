using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDeskKyotera.Migrations
{
    /// <inheritdoc />
    public partial class dddd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-1234-567890abcdef"),
                columns: new[] { "ConcurrencyStamp", "CreatedOn", "ModifiedOn", "PasswordHash", "SecurityStamp" },
                values: new object[] { "86212189-c83e-4d9e-a08b-e8c5ba90643d", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "AQAAAAIAAYagAAAAEPZbY6IG8bLCJjneEzhItdhP4tJ9J3hnJWnMcKx6Yj5GXiZXUEo9B/me+ZJgqoWMOA==", "admin-security-stamp-fixed-value" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-1234-567890abcdef"),
                columns: new[] { "ConcurrencyStamp", "CreatedOn", "ModifiedOn", "PasswordHash", "SecurityStamp" },
                values: new object[] { "2821ccd4-2708-4946-824c-11c34600f9fd", new DateTime(2025, 11, 27, 8, 8, 15, 255, DateTimeKind.Utc).AddTicks(189), new DateTime(2025, 11, 27, 8, 8, 15, 255, DateTimeKind.Utc).AddTicks(897), "AQAAAAIAAYagAAAAEJ3du5o4NKxINeq7/+ofdUpXrwML7d14R14JHbG+RanHZJh7GSbKP9CAhNMgu8WO4A==", "c8987590-9ed6-4aa8-8d4e-9804b63c3a5f" });
        }
    }
}
