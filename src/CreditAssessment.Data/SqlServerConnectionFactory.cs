using Microsoft.Data.SqlClient;

namespace CreditAssessment.Data;

public static class SqlServerConnectionFactory
{
    public static async Task<SqlConnection> OpenAsync()
    {
        string password = Environment.GetEnvironmentVariable("CREDIT_SQL_PASSWORD")
            ?? throw new InvalidOperationException(
                "Set the CREDIT_SQL_PASSWORD environment variable first.");

        var options = new SqlConnectionStringBuilder
        {
            DataSource = "127.0.0.1,14333",
            InitialCatalog = "CreditAssessment",
            UserID = "sa",
            Password = password,
            Encrypt = SqlConnectionEncryptOption.Mandatory,
            TrustServerCertificate = true
        };

        var connection = new SqlConnection(options.ConnectionString);

        try
        {
            await connection.OpenAsync();
            return connection;
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }
}