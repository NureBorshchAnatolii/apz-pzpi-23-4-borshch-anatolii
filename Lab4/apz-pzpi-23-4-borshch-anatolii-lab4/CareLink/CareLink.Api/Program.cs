using CareLink.Api.Middlewares;
using CareLink.Api.SwaggerConfigs;
using CareLink.Application;
using CareLink.Domain.Entities;
using CareLink.Persistence;
using CareLink.Persistence.DbContext;
using CareLink.Security;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

QuestPDF.Settings.License = LicenseType.Community;

builder.Host.UseSerilog((ctx, lc) =>
    lc.WriteTo.Console()
        .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddPersistenceServices(configuration);
builder.Services.AddApplicationServices();
builder.Services.AddSecurityServices(configuration);

builder.Services.AddControllers();

builder.Services.AddBearerSecurityScheme();

const string DevCors = "DevCors";
builder.Services.AddCors(o => o.AddPolicy(DevCors, p => p
    .SetIsOriginAllowed(_ => true)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));

var app = builder.Build();

const int maxMigrationAttempts = 30;
var migrationDelay = TimeSpan.FromSeconds(5);

for (var attempt = 1; attempt <= maxMigrationAttempts; attempt++)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CareLinkDbContext>();
        await dbContext.Database.MigrateAsync();

        var seedAdminPasswordHash = CareLinkDbContext.GenerateSeedPasswordHash(CareLinkDbContext.SeedAdminPassword);
        var seedAdmin = await dbContext.Users.SingleOrDefaultAsync(u => u.Email == CareLinkDbContext.SeedAdminEmail);

        if (seedAdmin is null)
        {
            dbContext.Users.Add(new User
            {
                Id = 1,
                FirstName = "System",
                LastName = "Administrator",
                RoleId = 1,
                Email = CareLinkDbContext.SeedAdminEmail,
                PasswordHash = seedAdminPasswordHash,
                DateOdBirth = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Address = "Kyiv, Admin Street 1",
                PhoneNumber = "+380501111111",
                DateCreated = DateTime.UtcNow
            });
        }
        else if (seedAdmin.PasswordHash != seedAdminPasswordHash)
        {
            seedAdmin.PasswordHash = seedAdminPasswordHash;
        }

        await dbContext.SaveChangesAsync();
        break;
    }
    catch (Exception ex) when (ex is SqlException || ex is InvalidOperationException || ex is DbUpdateException)
    {
        if (attempt == maxMigrationAttempts)
        {
            throw;
        }

        Log.Warning(ex, "Database migration attempt {Attempt}/{MaxAttempts} failed. Retrying in {DelaySeconds}s.", attempt, maxMigrationAttempts, migrationDelay.TotalSeconds);
        await Task.Delay(migrationDelay);
    }
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(DevCors);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseHttpsRedirection();

await app.RunAsync();
