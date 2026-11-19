using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicalGrpcService.Infra.Migrations
{
    /// <inheritdoc />
    public partial class AddPatient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "patients");

            migrationBuilder.CreateTable(
                name: "Patient",
                schema: "patients",
                columns: table => new
                {
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DoB = table.Column<string>(type: "text", nullable: false),
                    Sex = table.Column<short>(type: "smallint", nullable: true),
                    SexByBirth = table.Column<short>(type: "smallint", nullable: true),
                    Title = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    MRN = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    AddressLine1 = table.Column<string>(type: "text", nullable: true),
                    AddressLine2 = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    State = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Zip = table.Column<string>(type: "text", nullable: true),
                    MrnHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EmailHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PhoneHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordStatus = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patient", x => x.PatientId);
                });

            migrationBuilder.CreateTable(
                name: "PatientNameToken",
                schema: "patients",
                columns: table => new
                {
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientNameToken", x => new { x.OrganizationId, x.TokenHash, x.PatientId });
                    table.ForeignKey(
                        name: "FK_PatientNameToken_Patient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "patients",
                        principalTable: "Patient",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Patient_OrganizationId_MrnHash",
                schema: "patients",
                table: "Patient",
                columns: new[] { "OrganizationId", "MrnHash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PatientNameToken_PatientId",
                schema: "patients",
                table: "PatientNameToken",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PatientNameToken",
                schema: "patients");

            migrationBuilder.DropTable(
                name: "Patient",
                schema: "patients");
        }
    }
}
