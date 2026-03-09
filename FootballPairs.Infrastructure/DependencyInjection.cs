using FootballPairs.Application.Auth;
using FootballPairs.Application.Analytics;
using FootballPairs.Application.Import;
using FootballPairs.Application.Logging;
using FootballPairs.Application.MatchRecords;
using FootballPairs.Application.Matches;
using FootballPairs.Application.Players;
using FootballPairs.Application.Teams;
using FootballPairs.Infrastructure.Csv;
using FootballPairs.Infrastructure.Logging;
using FootballPairs.Infrastructure.Persistence;
using FootballPairs.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FootballPairs.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=FootballPairsDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        services.AddDbContext<FootballPairsDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRevokedTokenRepository, RevokedTokenRepository>();
        services.AddScoped<ITeamRepository, TeamRepository>();
        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<IMatchRepository, MatchRepository>();
        services.AddScoped<IMatchRecordRepository, MatchRecordRepository>();
        services.AddScoped<ICsvParser, CsvParser>();
        services.AddScoped<IHeaderMappingService, HeaderMappingService>();
        services.AddScoped<IDateParser, DateParser>();
        services.AddScoped<IImportTransaction, ImportTransaction>();
        services.AddScoped<IPrimaryRequestLogSink, DbRequestLogSink>();
        services.AddScoped<IFallbackRequestLogSink, FileRequestLogSink>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenRevocationService, TokenRevocationService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<ILogDataSanitizer, JsonLogDataSanitizer>();
        services.AddScoped<IRequestLogWriter, RequestLogWriter>();
        services.AddScoped<ITeamService, TeamService>();
        services.AddScoped<IPlayerService, PlayerService>();
        services.AddScoped<IMatchService, MatchService>();
        services.AddScoped<IMatchRecordService, MatchRecordService>();
        services.AddScoped<IImportService, ImportService>();

        return services;
    }
}
