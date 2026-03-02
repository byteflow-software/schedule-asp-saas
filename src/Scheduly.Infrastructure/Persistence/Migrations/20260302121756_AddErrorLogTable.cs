using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scheduly.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddErrorLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "error_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Level = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ExceptionType = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    StackTrace = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    Source = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RequestPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RequestMethod = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    RequestBody = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    HttpStatusCode = table.Column<int>(type: "integer", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExternalRequestUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ExternalResponseBody = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ExternalStatusCode = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_error_logs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_error_logs_Level",
                table: "error_logs",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_error_logs_TenantId",
                table: "error_logs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_error_logs_Timestamp",
                table: "error_logs",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "error_logs");
        }
    }
}
