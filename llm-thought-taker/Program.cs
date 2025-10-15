using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using llm_thought_taker.Data;
using Microsoft.Extensions.Configuration;
using GenerativeAI;
using llm_thought_taker.Endpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

// Authentication & Authorization
var clerkDomain = Environment.GetEnvironmentVariable("CLERK_DOMAIN")
    ?? builder.Configuration["ClerkDomain"];

if (string.IsNullOrEmpty(clerkDomain))
{
    throw new InvalidOperationException("Clerk domain is not configured. Set CLERK_DOMAIN environment variable or add to appsettings.json");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://{clerkDomain}";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            NameClaimType = "name",
        };
    });

builder.Services.AddAuthorization();

// Database configuration
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");


if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Database connection string is not configured. Set DB_CONNECTION_STRING environment variable or add to appsettings.json");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Generative AI
var geminiApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY")
    ?? builder.Configuration["GeminiApiKey"];

if (!string.IsNullOrEmpty(geminiApiKey))
{
    var googleAI = new GoogleAi(geminiApiKey);
    var model = googleAI.CreateGenerativeModel("models/gemini-2.0-flash-lite");
    builder.Services.AddSingleton(model);
}

var app = builder.Build();

app.UseCors();

// Map endpoints
app.MapGet("/health", () => {
    return Results.Ok(new { message = "I am just fine thanks" });
});

app.MapNotesEndpoints();
app.MapUsersEndpoints();

app.Run();