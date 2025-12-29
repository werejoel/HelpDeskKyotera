using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDeskKyotera.Migrations
{
    /// <inheritdoc />
    public partial class nnnnm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-1234-567890abcdef"),
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEH15Xv51WPxEy1L3cfe1dosnXpf6NHJtB1cBeJbrmLj0kIrqWkxAKCu8tpVrQqyTyw==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-1234-567890abcdef"),
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAECUThvG+YkOp9WhGz5k7SMiezQiLJSz2xVEfHI1xCDEmlIHUqL6B0aUB0Qb9BrDsaw==");
        }
    }
}
