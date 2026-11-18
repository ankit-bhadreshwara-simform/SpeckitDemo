using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SpeckitDemo.Data.Migrations
{
    /// <inheritdoc />
    public partial class Accounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Address", "CreatedDate", "Email", "FirstName", "LastName", "PasswordHash", "Username" },
                values: new object[,]
                {
                    { 1, "18 Harbor Lane, Port Merrin", new DateTime(2026, 3, 14, 0, 0, 0, 0, DateTimeKind.Utc), "mira.calder@speckitdemo.local", "Mira", "Calder", "$2a$12$MMliak12hVM4lTs0KW5FdeYYiEN6wJUGuEnsoI9EpEGg0zlmUDK6.", "mira" },
                    { 2, "42 Cedar Court, West Halden", new DateTime(2026, 3, 14, 0, 0, 0, 0, DateTimeKind.Utc), "nolan.briggs@speckitdemo.local", "Nolan", "Briggs", "$2a$12$YXiRCZVz8j0H41rVo/x9xOaJNU6qASaHD37Qc3ilpBvI2HuScz1FC", "nolan" },
                    { 3, "7 Lantern Street, Old Quarry", new DateTime(2026, 3, 14, 0, 0, 0, 0, DateTimeKind.Utc), "priya.raman@speckitdemo.local", "Priya", "Raman", "$2a$12$s8bbwMvHJ/GZoXoz5Crr4OGUsnH6/Brb0d9HSsYApoVyt5DPS./GK", "priya" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
