using Amazon.S3;
using MailKit.Net.Smtp;
using Marelli.Api.HealthChecks.Marelli.Api.HealthChecks;
using Marelli.Api.Middlewares;
using Marelli.Business.Clients;
using Marelli.Business.Factories;
using Marelli.Business.Hubs;
using Marelli.Business.IClients;
using Marelli.Business.IServices;
using Marelli.Business.Services;
using Marelli.Business.Utils;
using Marelli.Domain.Entities;
using Marelli.Infra.Configuration;
using Marelli.Infra.Context;
using Marelli.Infra.IRepositories;
using Marelli.Infra.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IAwsClient, AwsClient>();
builder.Services.AddSingleton<ISecretsManagerService, SecretsManagerService>();
builder.Services.AddDbContext<DemurrageContext>((serviceProvider, options) =>
{
    var secretsManagerService = serviceProvider.GetRequiredService<ISecretsManagerService>();

    var dbConnectionString = secretsManagerService.GetDbConnectionString().GetAwaiter().GetResult();

    options.UseNpgsql(dbConnectionString);
});
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<INewsService, NewsService>();
builder.Services.AddScoped<INewsRepository, NewsRepository>();
builder.Services.AddScoped<IEmailClient, EmailClient>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IUserTokenService, UserTokenService>();
builder.Services.AddScoped<IUserTokenRepository, UserTokenRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddHostedService<BuildStatusCheckerService>();
builder.Services.AddScoped<IBuildTableRowService, BuildTableRowService>();
builder.Services.AddScoped<IBuildTableRowRepository, BuildTableRowRepository>();
builder.Services.AddScoped<ICustomHttpClientFactory, CustomHttpClientFactory>();
builder.Services.AddScoped<IJenkinsRepository, JenkinsRepository>();
builder.Services.AddScoped<IJenkinsService, JenkinsService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IBuildLogRepository, BuildLogRepository>();
builder.Services.AddScoped<IBuildLogService, BuildLogService>();
builder.Services.AddScoped<IBaselineRepository, BaselineRepository>();
builder.Services.AddScoped<IBaselineService, BaselineService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<EncryptionUtils>();
builder.Services.AddScoped<FileUtils>();
builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddScoped<ISmtpClient, SmtpClient>();
builder.Services.AddHttpClient();
builder.Services.AddCors();
builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("SmtpOptions"));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
var appSettingsSection = builder.Configuration.GetSection("AppSettings");
builder.Services.Configure<AppSettings>(appSettingsSection);
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 100 * 1024 * 1024);

var appSettings = appSettingsSection.Get<AppSettings>();
var key = Encoding.ASCII.GetBytes(appSettings.Secret ?? "");
builder.Services
.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = true;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = appSettings.UrlHost,
        ValidIssuer = appSettings.Sender,
    };
});

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Insert the JWT token this way: Bearer {your token}",
        Name = "Authorization",
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.WithOrigins(
                "https://inenpla-app-devel.vonbraunlabs.com.br",
                "https://inenpla-app-homolog.vonbraunlabs.com.br",
                "https://inenpla-jenkins-devel.vonbraunlabs.com.br",
                "https://inenpla-jenkins-homolog.vonbraunlabs.com.br",
                "http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddHealthChecks()
    .AddCheck<DbHealthCheck>("Banco de Dados", tags: new[] { "db" })
    .AddCheck("App", () => HealthCheckResult.Healthy("Aplicação está funcionando"));


builder.Services.AddSingleton<ISecretsManagerService, SecretsManagerService>();


var app = builder.Build();

app.MapGet("/version", () =>
{
    var appInfo = new
    {
        Name = "Inenpla Api",
        Version = "1.0.0",
        Environment = app.Environment.EnvironmentName,
        Timestamp = DateTime.UtcNow
    };
    return Results.Json(appInfo);
});

app.MapHealthChecks("/healthcheck", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                exception = e.Value.Exception?.Message,
                duration = e.Value.Duration.TotalMilliseconds
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds
        };

        await context.Response.WriteAsJsonAsync(response);
    }
});

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowSpecificOrigin");
app.UseHttpsRedirection();
app.UseMiddleware<TokenValidationMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<BuildStateHub>("/buildingStateHub");
app.MapHub<BuildTableHub>("/buildTableHub");
app.MapHub<BuildLogHub>("/buildLogHub");
app.MapHub<UploadProgressHub>("/uploadProgressHub");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
    await dbContext.Database.MigrateAsync();
}

app.Run();
public partial class Program { }