using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendanceManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnrollmentActiveGroupIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_enrollment_active_group",
                table: "enrollment",
                column: "transport_group_id",
                filter: "active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_enrollment_active_group",
                table: "enrollment");
        }
    }
}
