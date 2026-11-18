using System.Text;
using Clube.Api.Middleware;
using Clube.Infrastructure;
using Clube.Infrastructure.Persistence;
using Clube.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddClubeInfrastructure(
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? DependencyInjection.ResolveLocalDatabasePath(builder.Environment.ContentRootPath));

var jwtKey = builder.Configuration["Jwt:SecretKey"] ?? JwtTokenService.DefaultDevKey;
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "Clube.Api";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "Clube.Clients";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ClubeDbContext>();
    await db.Database.EnsureCreatedAsync();
    await DbInitializer.SeedAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Modo local HTTP: não redirecionar para HTTPS para evitar falhas no WebAdmin e MAUI em localhost:5000.
app.UseCors();
app.UseAuthentication();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;
