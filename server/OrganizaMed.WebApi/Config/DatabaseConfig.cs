using OrganizaMed.Infraestrutura.Orm.Compartilhado;

namespace OrganizaMed.WebApi.Config;

public static class DatabaseConfig
{
    public static bool AutoMigrateDatabase(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<OrganizaMedDbContext>();

        var migracaoConcluida = MigradorBancoDados.AtualizarBancoDados(dbContext);

        return migracaoConcluida;
    }
}