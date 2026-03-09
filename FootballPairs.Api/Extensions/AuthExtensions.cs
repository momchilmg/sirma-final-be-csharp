using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FootballPairs.Application.Auth;
using FootballPairs.Application.Common.Errors;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace FootballPairs.Api.Extensions;

public static class AuthExtensions
{
    public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtKey = configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            throw new InvalidOperationException("JWT signing key is not configured.");
        }

        var jwtIssuer = configuration["Jwt:Issuer"] ?? "FootballPairs.Api";
        var jwtAudience = configuration["Jwt:Audience"] ?? "FootballPairs.Client";
        var usernameClaimType = configuration["Jwt:UsernameClaimType"] ?? JwtRegisteredClaimNames.UniqueName;
        var roleClaimType = configuration["Jwt:RoleClaimType"] ?? ClaimTypes.Role;
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    NameClaimType = usernameClaimType,
                    RoleClaimType = roleClaimType
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var jti = context.Principal?.FindFirstValue(JwtRegisteredClaimNames.Jti);
                        if (string.IsNullOrWhiteSpace(jti))
                        {
                            context.Fail("Token is missing jti claim.");
                            return;
                        }

                        var tokenRevocationService = context.HttpContext.RequestServices.GetRequiredService<ITokenRevocationService>();
                        if (await tokenRevocationService.IsRevokedAsync(jti, context.HttpContext.RequestAborted))
                        {
                            context.Fail("Token has been revoked.");
                        }
                    },
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        return WriteAuthProblemDetailsAsync(
                            context.HttpContext,
                            StatusCodes.Status401Unauthorized,
                            "Authentication is required.",
                            ErrorCodes.Unauthenticated);
                    },
                    OnForbidden = context =>
                    {
                        return WriteAuthProblemDetailsAsync(
                            context.HttpContext,
                            StatusCodes.Status403Forbidden,
                            "Access is forbidden.",
                            ErrorCodes.Forbidden);
                    }
                };
            });

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        return services;
    }

    private static Task WriteAuthProblemDetailsAsync(HttpContext context, int statusCode, string title, string errorCode)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = GetTypeUri(statusCode),
            Instance = context.Request.Path
        };
        problemDetails.Extensions["traceId"] = traceId;
        problemDetails.Extensions["errorCode"] = errorCode;
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        return context.Response.WriteAsJsonAsync(problemDetails);
    }

    private static string GetTypeUri(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status401Unauthorized => "https://tools.ietf.org/html/rfc9110#section-15.5.2",
            StatusCodes.Status403Forbidden => "https://tools.ietf.org/html/rfc9110#section-15.5.4",
            _ => "https://tools.ietf.org/html/rfc9110#section-15.6.1"
        };
    }
}
