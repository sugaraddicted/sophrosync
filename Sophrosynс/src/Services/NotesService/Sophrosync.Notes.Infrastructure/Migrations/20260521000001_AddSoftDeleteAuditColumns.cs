using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sophrosync.Notes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteAuditColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByUserId",
                table: "notes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByFullName",
                table: "notes",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "notes");

            migrationBuilder.DropColumn(
                name: "DeletedByFullName",
                table: "notes");
        }
    }
}
