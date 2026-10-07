using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Mue2MueDee.Application;
using Mue2MueDee.Application.Common.Interfaces;
using Mue2MueDee.Infrastructure;
using Mue2MueDee.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add Application & Infrastructure Clean Architecture Layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add Controllers & JSON Options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// OpenAPI
builder.Services.AddOpenApi();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// JWT Authentication
var jwtSecret = builder.Configuration["Jwt:SecretKey"] ?? "Mue2MueDeeSuperSecretSecurityKey2026!@#$%^&*()_+DefaultKeyForDevOnly";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "Mue2MueDee";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "Mue2MueDeeApp";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();
builder.Services.AddHealthChecks();

var app = builder.Build();

// Auto-seed database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await DbInitializer.SeedAsync(context, passwordHasher, logger);
}

// HTTP request pipeline
// Global Exception Handling Middleware
app.UseMiddleware<Mue2MueDee.Web.Middleware.ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapGet("/api/health", () => Results.Ok(new 
{ 
    status = "Healthy", 
    timestamp = DateTime.UtcNow, 
    system = "MUE2MUEDEE API",
    version = "1.0.0-phase1"
}))
.WithName("HealthCheck");

app.MapControllers();

app.Run();
