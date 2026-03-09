CREATE OR ALTER TRIGGER [dbo].[TR_RevokedTokens_DeleteExpiredAfter24Hours]
ON [dbo].[RevokedTokens]
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM [dbo].[RevokedTokens]
    WHERE [ExpiresAtUtc] <= DATEADD(HOUR, -24, SYSUTCDATETIME());
END
