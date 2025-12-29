using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDeskKyotera.Migrations
{
    /// <inheritdoc />
    public partial class mmmm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClaimMasters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ClaimValue = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaimMasters", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-1234-567890abcdef"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "16932416-d860-4d30-a60b-ff1a8fc35202", "AQAAAAIAAYagAAAAEHGYwv5JsvXnsBX34BSDkoLUeqqSlc6B3A7oQn1WBdAw49eX9/2FcnUjUmahg0ntRg==" });

            migrationBuilder.CreateIndex(
                name: "UX_ClaimMasters_TypeValueCategory",
                table: "ClaimMasters",
                columns: new[] { "ClaimType", "ClaimValue", "Category" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClaimMasters");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-1234-567890abcdef"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "9828cfa7-6879-4c30-9e70-4b82a9f5b1c4", "AQAAAAIAAYagAAAAEGRb7EY1haGe0NGdNTIJCU6w/hHmvjwu38RLrALxlWJCW7TN2awksUNq6QtcbPULig==" });
        }
    }
}
