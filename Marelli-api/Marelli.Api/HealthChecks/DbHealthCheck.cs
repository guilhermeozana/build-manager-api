namespace Marelli.Api.HealthChecks
{
    using global::Marelli.Infra.Context;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using System;
    using System.Threading.Tasks;

    namespace Marelli.Api.HealthChecks
    {
        public class DbHealthCheck : IHealthCheck
        {
            private readonly IServiceProvider _serviceProvider;

            public DbHealthCheck(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                        var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

                        if (canConnect)
                        {
                            return HealthCheckResult.Healthy("Banco de dados está funcionando.");
                        }
                        else
                        {
                            return HealthCheckResult.Unhealthy("Não foi possível conectar ao banco de dados.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    return HealthCheckResult.Unhealthy($"Falha na conexão com o banco de dados: {ex.Message}");
                }
            }
        }
    }

}
