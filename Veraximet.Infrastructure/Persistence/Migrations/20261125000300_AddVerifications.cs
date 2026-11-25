using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veraximet.Infrastructure.Persistence.Migrations
{
    public partial class AddVerifications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Verifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    InstrumentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    PerformedBy = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Unit = table.Column<string>(type: "TEXT", maxLength: 24, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    DecisionRule = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Result = table.Column<string>(type: "TEXT", maxLength: 24, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Verifications", x => x.Id);
                    table.ForeignKey("FK_Verifications_Instruments_InstrumentId", x => x.InstrumentId, "Instruments", "Id", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VerificationPoints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    VerificationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    ReferenceValue = table.Column<decimal>(type: "TEXT", nullable: false),
                    MaximumPermissibleError = table.Column<decimal>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerificationPoints", x => x.Id);
                    table.ForeignKey("FK_VerificationPoints_Verifications_VerificationId", x => x.VerificationId, "Verifications", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Measurements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    VerificationPointId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Measurements", x => x.Id);
                    table.ForeignKey("FK_Measurements_VerificationPoints_VerificationPointId", x => x.VerificationPointId, "VerificationPoints", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex("IX_Verifications_InstrumentId_Date", "Verifications", new[] { "InstrumentId", "Date" });
            migrationBuilder.CreateIndex("IX_VerificationPoints_VerificationId_Order", "VerificationPoints", new[] { "VerificationId", "Order" }, unique: true);
            migrationBuilder.CreateIndex("IX_Measurements_VerificationPointId_Order", "Measurements", new[] { "VerificationPointId", "Order" }, unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("Measurements");
            migrationBuilder.DropTable("VerificationPoints");
            migrationBuilder.DropTable("Verifications");
        }
    }
}
