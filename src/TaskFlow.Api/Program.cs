using TaskFlow.Infrastructure;
using TaskFlow.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// --- Services (composition root) ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Wire the Infrastructure layer (data store, hashing, db initializer).
builder.Services.AddInfrastructure(builder.Configuration);

// Allow the Vite dev server (separate origin) to call the API during development.
const string SpaCorsPolicy = "spa";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                     ?? new[] { "http://localhost:5173" };
builder.Services.AddCors(options =>
    options.AddPolicy(SpaCorsPolicy, policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// --- Create/seed the database on startup ---
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<DbInitializer>().Initialize();
}

// --- Middleware pipeline ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(SpaCorsPolicy);

// Serve the built React SPA (client/dist copied to wwwroot) in production.
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

// Any non-API route falls back to the SPA host page so client-side routing works.
// This is the .NET endpoint that serves the React single-page application.
app.MapFallbackToFile("index.html");

app.Run();

// Exposed so the integration/functional test host can reference the entry point.
public partial class Program { }
