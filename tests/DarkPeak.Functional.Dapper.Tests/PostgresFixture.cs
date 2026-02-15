using Npgsql;
using Testcontainers.PostgreSql;
using TUnit.Core.Interfaces;

namespace DarkPeak.Functional.Dapper.Tests;

public class PostgresFixture : IAsyncInitializer, IAsyncDisposable
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine")
        .Build();

    private NpgsqlConnection? _connection;

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        _connection = new NpgsqlConnection(ConnectionString);
        await _connection.OpenAsync();

        await using var cmd = _connection.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE products (
                id SERIAL PRIMARY KEY,
                name TEXT NOT NULL,
                price NUMERIC(10,2) NOT NULL
            );

            CREATE TABLE users (
                id SERIAL PRIMARY KEY,
                email TEXT NOT NULL UNIQUE,
                name TEXT NOT NULL
            );

            CREATE TABLE accounts (
                id SERIAL PRIMARY KEY,
                name TEXT NOT NULL,
                balance NUMERIC(10,2) NOT NULL DEFAULT 0
            );

            INSERT INTO products (name, price) VALUES
                ('Widget', 9.99),
                ('Gadget', 24.99),
                ('Doohickey', 4.99);

            INSERT INTO users (email, name) VALUES
                ('alice@example.com', 'Alice'),
                ('bob@example.com', 'Bob');

            INSERT INTO accounts (name, balance) VALUES
                ('Alice', 100.00),
                ('Bob', 50.00);
            """;
        await cmd.ExecuteNonQueryAsync();
        await _connection.CloseAsync();
    }

    public async Task<NpgsqlConnection> OpenConnectionAsync()
    {
        var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        return connection;
    }

    public NpgsqlConnection CreateClosedConnection()
    {
        return new NpgsqlConnection(ConnectionString);
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null) await _connection.DisposeAsync();
        await _container.DisposeAsync();
    }
}
