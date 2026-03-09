using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballPairs.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnforceSingleMatchRecordPerPlayer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MatchRecords_MatchId_PlayerId",
                table: "MatchRecords");

            migrationBuilder.CreateIndex(
                name: "IX_MatchRecords_MatchId_PlayerId",
                table: "MatchRecords",
                columns: new[] { "MatchId", "PlayerId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MatchRecords_MatchId_PlayerId",
                table: "MatchRecords");

            migrationBuilder.CreateIndex(
                name: "IX_MatchRecords_MatchId_PlayerId",
                table: "MatchRecords",
                columns: new[] { "MatchId", "PlayerId" });
        }
    }
}
