using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicalGrpcService.Infra.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientRecording : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PatientRecording",
                schema: "patients",
                columns: table => new
                {
                    RecordingId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordStatus = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientRecording", x => x.RecordingId);
                    table.ForeignKey(
                        name: "FK_PatientRecording_Patient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "patients",
                        principalTable: "Patient",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PatientRecording_PatientId",
                schema: "patients",
                table: "PatientRecording",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PatientRecording",
                schema: "patients");
        }
    }
}
