using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballPairs.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRevokedTokensCleanupTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE OR ALTER TRIGGER [dbo].[TR_RevokedTokens_DeleteExpiredAfter24Hours]
                ON [dbo].[RevokedTokens]
                AFTER INSERT
                AS
                BEGIN
                    SET NOCOUNT ON;

                    DELETE FROM [dbo].[RevokedTokens]
                    WHERE [ExpiresAtUtc] <= DATEADD(HOUR, -24, SYSUTCDATETIME());
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [dbo].[TR_RevokedTokens_DeleteExpiredAfter24Hours];");
        }
    }
}
