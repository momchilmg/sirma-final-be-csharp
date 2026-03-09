using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballPairs.Infrastructure.Persistence.Migrations
{
    public partial class AddMatchRecords : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MatchRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MatchId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    FromMinute = table.Column<int>(type: "int", nullable: false),
                    ToMinute = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchRecords_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MatchRecords_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql(
                """
                ALTER TABLE [MatchRecords]
                ADD CONSTRAINT [CK_MatchRecords_Minutes]
                CHECK ([FromMinute] >= 0 AND ([ToMinute] IS NULL OR [FromMinute] < [ToMinute]));
                """);

            migrationBuilder.Sql(
                """
                CREATE OR ALTER TRIGGER [dbo].[TR_MatchRecords_NoOverlap]
                ON [dbo].[MatchRecords]
                AFTER INSERT, UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;

                    IF EXISTS
                    (
                        SELECT 1
                        FROM inserted i
                        INNER JOIN [dbo].[Matches] m ON m.Id = i.MatchId
                        INNER JOIN [dbo].[MatchRecords] r
                            ON r.MatchId = i.MatchId
                            AND r.PlayerId = i.PlayerId
                            AND r.Id <> i.Id
                        WHERE i.FromMinute < ISNULL(r.ToMinute, ISNULL(m.EndMinute, 90))
                            AND r.FromMinute < ISNULL(i.ToMinute, ISNULL(m.EndMinute, 90))
                    )
                    OR EXISTS
                    (
                        SELECT 1
                        FROM inserted i1
                        INNER JOIN [dbo].[Matches] m1 ON m1.Id = i1.MatchId
                        INNER JOIN inserted i2
                            ON i1.MatchId = i2.MatchId
                            AND i1.PlayerId = i2.PlayerId
                            AND i1.Id < i2.Id
                        INNER JOIN [dbo].[Matches] m2 ON m2.Id = i2.MatchId
                        WHERE i1.FromMinute < ISNULL(i2.ToMinute, ISNULL(m2.EndMinute, 90))
                            AND i2.FromMinute < ISNULL(i1.ToMinute, ISNULL(m1.EndMinute, 90))
                    )
                    BEGIN
                        THROW 51000, 'MATCH_RECORD_INTERVAL_OVERLAP', 1;
                    END
                END
                """);

            migrationBuilder.CreateIndex(
                name: "IX_MatchRecords_MatchId_PlayerId",
                table: "MatchRecords",
                columns: new[] { "MatchId", "PlayerId" });

            migrationBuilder.CreateIndex(
                name: "IX_MatchRecords_PlayerId",
                table: "MatchRecords",
                column: "PlayerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [dbo].[TR_MatchRecords_NoOverlap];");

            migrationBuilder.DropTable(
                name: "MatchRecords");
        }
    }
}
