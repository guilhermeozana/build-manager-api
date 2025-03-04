using Marelli.Business.Services;
//using Marelli.Business.Validations;
//using Marelli.Business.Services.FreeTime;
using Marelli.Infra.Context;
using Marelli.Infra.Repositories;
//using Marelli.Infra.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Marelli.Api.Configuration;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
//'using Hangfire;

var builder = WebApplication.CreateBuilder(args);

/*builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("AZURE_SQL_CONECTIONSTRING")));

builder.Services.AddHangfireServer();*/
// Add services to the container.
//builder.Services.AddDbContext<DemurrageContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("AZURE_SQL_CONECTIONSTRING")));




    builder.Services.AddEntityFrameworkNpgsql()
    .AddDbContext<DemurrageContext>(options =>
    options.UseNpgsql("Host=localhost;Port=5432;Pooling=true;Database=TesteVonbatra;User Id=postgres;Password=!098Cao10@;"));


//builder.Services.AddHostedService<ContainerJobWorker>();  // Adiciona o worker como um serviço hospedado.

//builder.Services.AddScoped(typeof(DemurrageContext), typeof(DemurrageContext));
//builder.Services.AddScoped<IArmadorRepository, ArmadorRepository>();
//builder.Services.AddScoped(typeof(ArmadorService), typeof(ArmadorService));

//builder.Services.AddScoped(typeof(TransportadorService), typeof(TransportadorService));
//builder.Services.AddScoped(typeof(ITransportadorRepository), typeof(TransportadorRepository));

//builder.Services.AddScoped(typeof(ContainerRepository), typeof(ContainerRepository));
//builder.Services.AddScoped(typeof(ContainerService), typeof(ContainerService));
//builder.Services.AddScoped(typeof(ContainerJobService), typeof(ContainerJobService));
//builder.Services.AddScoped(typeof(ArmadorValidation), typeof(ArmadorValidation));

//builder.Services.AddScoped(typeof(AgenteCargaRepository), typeof(AgenteCargaRepository));
//builder.Services.AddScoped(typeof(AgenteCargaService), typeof(AgenteCargaService));

//builder.Services.AddScoped(typeof(ContinenteRepository), typeof(ContinenteRepository));
//builder.Services.AddScoped(typeof(ContinenteService), typeof(ContinenteService));

//builder.Services.AddScoped(typeof(IFreeTimeRepository), typeof(FreeTimeRepository));
//builder.Services.AddScoped(typeof(FreeTimeService), typeof(FreeTimeService));

//builder.Services.AddScoped(typeof(IPortoRepository), typeof(PortoRepository));
//builder.Services.AddScoped(typeof(IPortoService), typeof(PortoService));

builder.Services.AddScoped(typeof(BuildTableRowsRepository), typeof(BuildTableRowsRepository));
builder.Services.AddScoped(typeof(BuildTableRowsServices), typeof(BuildTableRowsServices));

builder.Services.AddScoped(typeof(UsuarioRepository), typeof(UsuarioRepository));
builder.Services.AddScoped(typeof(UsuarioService), typeof(UsuarioService));

builder.Services.AddCors();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
var appSettingsSection = builder.Configuration.GetSection("AppSettings");
builder.Services.Configure<AppSettings>(appSettingsSection);
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
        ValidAudience = appSettings.ValidoEm,
        ValidIssuer = appSettings.Emissor,
    };
});

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Insira o token JWT desta maneira: Bearer {seu token}",
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

var app = builder.Build();

// Configure the HTTP request pipeline.
// Com essa verificação abaixo o swagger ficaria indisponível em prd
// if (app.Environment.IsDevelopment()) 
// {
app.UseSwagger();
app.UseSwaggerUI();
// }

app.UseCors(x => x
.AllowAnyOrigin()
.AllowAnyMethod()
.AllowAnyHeader()
);

// Para testes locais em contêiner comentar a instrução app.UseHttpsRedirectionp
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
