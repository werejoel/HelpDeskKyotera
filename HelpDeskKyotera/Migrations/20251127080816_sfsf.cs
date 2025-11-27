using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDeskKyotera.Migrations
{
    /// <inheritdoc />
    public partial class sfsf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-1234-567890abcdef"),
                columns: new[] { "ConcurrencyStamp", "CreatedOn", "ModifiedOn", "PasswordHash", "SecurityStamp" },
                values: new object[] { "2821ccd4-2708-4946-824c-11c34600f9fd", new DateTime(2025, 11, 27, 8, 8, 15, 255, DateTimeKind.Utc).AddTicks(189), new DateTime(2025, 11, 27, 8, 8, 15, 255, DateTimeKind.Utc).AddTicks(897), "AQAAAAIAAYagAAAAEJ3du5o4NKxINeq7/+ofdUpXrwML7d14R14JHbG+RanHZJh7GSbKP9CAhNMgu8WO4A==", "c8987590-9ed6-4aa8-8d4e-9804b63c3a5f" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-1234-567890abcdef"),
                columns: new[] { "ConcurrencyStamp", "CreatedOn", "ModifiedOn", "PasswordHash", "SecurityStamp" },
                values: new object[] { "987de748-f182-488c-b882-f23194ccbc1b", new DateTime(2025, 11, 26, 16, 18, 21, 818, DateTimeKind.Utc).AddTicks(2817), new DateTime(2025, 11, 26, 16, 18, 21, 818, DateTimeKind.Utc).AddTicks(3276), "AQAAAAIAAYagAAAAEFbu5uuKBmrCwCleSZcmQ/MrYYIoOCdfx8MdulQR9oQDMmQcyOAq9BRriYL4XSKzRA==", "0ddaa9ac-e1b1-4124-ad6f-926bd9c64cad" });
        }
    }
}
