using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using LegacyApp.Domain;

namespace LegacyApp.Infrastructure.SqlServer;

public class ClientRepository : IClientRepository, IDisposable
{
    private readonly SqlConnection connection;
    private bool disposed;
    private bool createdConnection;

    public ClientRepository(SqlConnection? connection = null)
    {
        if (connection == null)
        {
            this.connection = new SqlConnection(ConfigurationManager.ConnectionStrings["appDatabase"].ConnectionString);
            createdConnection = true;
        }
        else
        {
            this.connection = connection;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                // dispose of connection IF we created it.
                if (createdConnection)
                {
                    connection?.Dispose();
                }
            }
            disposed = true;
        }
    }

    public async Task<Client?> GetByIdAsync(int id)
    {
        Client? client = null;

        await using SqlCommand command = new()
        {
            Connection = connection,
            CommandType = CommandType.StoredProcedure,
            CommandText = "uspGetClientById"
        };

        SqlParameter parameter = new("@ClientId", SqlDbType.Int) { Value = id };
        command.Parameters.Add(parameter);

        await connection.OpenAsync();
        await using SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        if (reader.HasRows)
        {
            await reader.ReadAsync();

            client = new Client
            {
                Id = int.Parse(await reader.GetFieldValueAsync<string>("ClientId")),
                Name = await reader.GetFieldValueAsync<string>("Name"),
                ClientStatus = (ClientStatus)int.Parse(await reader.GetFieldValueAsync<string>("ClientStatusId"))
            };
        }

        return client;
    }
}
