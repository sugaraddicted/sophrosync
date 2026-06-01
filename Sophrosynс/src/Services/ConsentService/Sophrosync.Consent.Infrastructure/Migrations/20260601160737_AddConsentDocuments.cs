using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sophrosync.Consent.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConsentDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "consent_documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsentRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Data = table.Column<byte[]>(type: "bytea", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_consent_documents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_consent_documents_ConsentRecordId",
                table: "consent_documents",
                column: "ConsentRecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_consent_documents_TenantId",
                table: "consent_documents",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "consent_documents");
        }
    }
}
