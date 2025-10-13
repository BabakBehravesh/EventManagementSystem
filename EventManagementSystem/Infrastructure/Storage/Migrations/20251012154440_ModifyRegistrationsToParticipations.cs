using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class ModifyRegistrationsToParticipations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_Events_EventId",
                table: "Registrations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Registrations",
                table: "Registrations");

            migrationBuilder.RenameTable(
                name: "Registrations",
                newName: "Participations");

            migrationBuilder.RenameIndex(
                name: "IX_Registrations_EventId_Email",
                table: "Participations",
                newName: "IX_Participations_EventId_Email");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Participations",
                table: "Participations",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Participations_Events_EventId",
                table: "Participations",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Participations_Events_EventId",
                table: "Participations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Participations",
                table: "Participations");

            migrationBuilder.RenameTable(
                name: "Participations",
                newName: "Registrations");

            migrationBuilder.RenameIndex(
                name: "IX_Participations_EventId_Email",
                table: "Registrations",
                newName: "IX_Registrations_EventId_Email");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Registrations",
                table: "Registrations",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_Events_EventId",
                table: "Registrations",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
