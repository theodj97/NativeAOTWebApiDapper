using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Testcontainers.MsSql;

namespace WebApiDapperNativeAOT.Testing.SeedWork;

public class TestServerFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    public HttpClient HttpClient { get; private set; }
    public TestServerFixtureExtension Extension;
    private readonly MsSqlContainer sqlServerContainer;
    private static string sqlServerCnnString = string.Empty;
    public TestServerFixture()
    {
        sqlServerContainer = new MsSqlBuilder().WithImage("mcr.microsoft.com/mssql/server:2022-CU13-ubuntu-20.04").Build();
        sqlServerContainer.Started += (sender, args) =>
        {
            InitDatabase((sender as MsSqlContainer).GetConnectionString());
        };
    }

    private static void InitDatabase(string sqlConnectionStr)
    {
        var sql = File.ReadAllText("Setup/Data/create-database.sql");
        using SqlConnection conn = new(sqlConnectionStr);
        Server server = new(new ServerConnection(conn));
        server.ConnectionContext.ExecuteNonQuery(sql);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var factory = new WebApplicationFactory<Program>();
        builder.ConfigureAppConfiguration((context, builder) =>
        {
            builder.Sources.Clear();
        });
        builder.ConfigureTestServices(services =>
        {
            var sqlCnnstringServiceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(SqlConnection));
            if (sqlCnnstringServiceDescriptor is not null)
                services.Remove(sqlCnnstringServiceDescriptor);

            services.AddTransient(_ => new SqlConnection(sqlServerCnnString));
        });
    }

    public async Task InitializeAsync()
    {
        await sqlServerContainer.StartAsync();

        sqlServerCnnString =
        sqlServerContainer.GetConnectionString().Replace("Database=master", "Database=Todo");
        HttpClient = Server.CreateClient();
        Extension = new TestServerFixtureExtension(sqlServerCnnString);
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        return sqlServerContainer.DisposeAsync().AsTask();
    }

    internal static async Task ResetDatabase()
    {
        using SqlConnection conn = new(sqlServerCnnString);
        await conn.OpenAsync();

        var resetSql = @"
            DELETE FROM dbo.Todos;
            DBCC CHECKIDENT ('Todos', RESEED, 0);";

        using SqlCommand cmd = new(resetSql, conn);
        await cmd.ExecuteNonQueryAsync();
    }
}
