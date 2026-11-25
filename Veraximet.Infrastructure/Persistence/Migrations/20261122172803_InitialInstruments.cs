using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veraximet.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialInstruments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Instruments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false, collation: "NOCASE"),
                    Name = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Manufacturer = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true),
                    Model = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true),
                    SerialNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Department = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Location = table.Column<string>(type: "TEXT", maxLength: 160, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    VerificationIntervalMonths = table.Column<int>(type: "INTEGER", nullable: false),
                    LastVerificationDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    NextDueDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    OperationalStatus = table.Column<string>(type: "TEXT", maxLength: 24, nullable: false),
                    ConformityStatus = table.Column<string>(type: "TEXT", maxLength: 24, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instruments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Instruments_Category",
                table: "Instruments",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Instruments_Code",
                table: "Instruments",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Instruments_Department",
                table: "Instruments",
                column: "Department");

            migrationBuilder.CreateIndex(
                name: "IX_Instruments_NextDueDate",
                table: "Instruments",
                column: "NextDueDate");
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Instruments");
        }
    }
}
