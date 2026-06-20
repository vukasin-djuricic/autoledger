using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoLedger.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReversalLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReversalOfEntryId",
                table: "JournalEntries",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_ReversalOfEntryId",
                table: "JournalEntries",
                column: "ReversalOfEntryId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_JournalEntries_JournalEntries_ReversalOfEntryId",
                table: "JournalEntries",
                column: "ReversalOfEntryId",
                principalTable: "JournalEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JournalEntries_JournalEntries_ReversalOfEntryId",
                table: "JournalEntries");

            migrationBuilder.DropIndex(
                name: "IX_JournalEntries_ReversalOfEntryId",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "ReversalOfEntryId",
                table: "JournalEntries");
        }
    }
}
