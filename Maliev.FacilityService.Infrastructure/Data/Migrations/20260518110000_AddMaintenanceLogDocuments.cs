using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maliev.FacilityService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMaintenanceLogDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "equipment_maintenance_documents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    maintenance_log_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    content_type = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    file_size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    storage_path = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    uploaded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_equipment_maintenance_documents", x => x.id);
                    table.ForeignKey(
                        name: "FK_equipment_maintenance_documents_logs",
                        column: x => x.maintenance_log_id,
                        principalTable: "equipment_maintenance_logs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_equipment_maintenance_documents_maintenance_log_id",
                table: "equipment_maintenance_documents",
                column: "maintenance_log_id");

            migrationBuilder.CreateIndex(
                name: "IX_equipment_maintenance_documents_storage_path",
                table: "equipment_maintenance_documents",
                column: "storage_path");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "equipment_maintenance_documents");
        }
    }
}
