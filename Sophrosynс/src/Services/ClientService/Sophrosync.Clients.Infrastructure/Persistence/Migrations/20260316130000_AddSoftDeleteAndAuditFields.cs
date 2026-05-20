using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sophrosync.Clients.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteAndAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Soft-delete columns
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "clients",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "clients",
                type: "timestamptz",
                nullable: true);

            // Audit timestamp — back-fill with CreatedAt for existing rows
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "clients",
                type: "timestamptz",
                nullable: false,
                defaultValueSql: "now()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "clients");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "clients");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "clients");
        }
    }
}
