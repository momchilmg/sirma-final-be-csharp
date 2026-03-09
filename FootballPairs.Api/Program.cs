using System.Diagnostics;
using FootballPairs.Api.Configuration;
using FootballPairs.Api.Extensions;
using FootballPairs.Api.Middleware;
using FootballPairs.Application.Common.Errors;
using FootballPairs.Infrastructure;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAuth(builder.Configuration);
builder.Services.Configure<ImportPathOptions>(builder.Configuration.GetSection(ImportPathOptions.SectionName));
builder.Services.AddControllers();

// ATTENTION! CORS policy is configured to allow all origins, methods, and headers for development purposes only.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddProblemDetails();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var traceId = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
        var problemDetails = new ValidationProblemDetails(context.ModelState)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred.",
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            Instance = context.HttpContext.Request.Path
        };
        problemDetails.Extensions["traceId"] = traceId;
        problemDetails.Extensions["errorCode"] = ErrorCodes.ValidationFailed;
        return new BadRequestObjectResult(problemDetails)
        {
            ContentTypes = { "application/problem+json" }
        };
    };
});

var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
