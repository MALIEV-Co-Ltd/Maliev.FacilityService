using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maliev.FacilityService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCompositeIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_equipment_notes_equipment_id_created_at",
                table: "equipment_notes",
                columns: new[] { "equipment_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_equipment_loans_equipment_id_loan_status",
                table: "equipment_loans",
                columns: new[] { "equipment_id", "loan_status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_equipment_notes_equipment_id_created_at",
                table: "equipment_notes");

            migrationBuilder.DropIndex(
                name: "IX_equipment_loans_equipment_id_loan_status",
                table: "equipment_loans");
        }
    }
}
