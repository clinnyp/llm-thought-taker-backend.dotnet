using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using llm_thought_taker.Data;
using Microsoft.Extensions.Hosting;
using GenerativeAI;

var builder = FunctionsApplication.CreateBuilder(args);
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
});

var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var geminiApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
var googleAI = new GoogleAi(geminiApiKey);
var model = googleAI.CreateGenerativeModel("models/gemini-2.0-flash-lite");

builder.Services.AddSingleton(model);

builder.Build().Run();