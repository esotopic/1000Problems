using Microsoft.Data.SqlClient;
using _1000Problems.Models;

namespace _1000Problems.Data;

public class ApplicationRepository
{
    private readonly string _connectionString;

    public ApplicationRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private SqlConnection CreateConnection(int timeoutSeconds = 120)
    {
        var builder = new SqlConnectionStringBuilder(_connectionString)
        {
            ConnectTimeout = timeoutSeconds
        };
        return new SqlConnection(builder.ConnectionString);
    }

    public async Task<List<Application>> GetActiveApplicationsAsync(string? searchTerm = null)
    {
        var apps = new List<Application>();
        using var connection = CreateConnection();
        await connection.OpenAsync();

        var sql = @"SELECT Id, Name, Description, Notes, Url, ImageUrl, IsActive, CreatedDate, ModifiedDate
                    FROM Applications WHERE IsActive = 1";

        if (!string.IsNullOrWhiteSpace(searchTerm))
            sql += " AND (Name LIKE @Search OR Description LIKE @Search OR Notes LIKE @Search)";

        sql += " ORDER BY CreatedDate DESC";

        using var command = new SqlCommand(sql, connection);
        if (!string.IsNullOrWhiteSpace(searchTerm))
            command.Parameters.AddWithValue("@Search", $"%{searchTerm}%");

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            apps.Add(new Application
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2),
                Notes = reader.IsDBNull(3) ? null : reader.GetString(3),
                Url = reader.IsDBNull(4) ? null : reader.GetString(4),
                ImageUrl = reader.IsDBNull(5) ? null : reader.GetString(5),
                IsActive = reader.GetBoolean(6),
                CreatedDate = reader.GetDateTime(7),
                ModifiedDate = reader.GetDateTime(8)
            });
        }
        return apps;
    }

    public async Task EnsureTableExistsAsync()
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        var sql = @"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Applications' AND xtype='U')
            BEGIN
                CREATE TABLE Applications (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    Name NVARCHAR(200) NOT NULL,
                    Description NVARCHAR(1000) NOT NULL DEFAULT '',
                    Notes NVARCHAR(MAX) NULL,
                    Url NVARCHAR(500) NULL,
                    ImageUrl NVARCHAR(500) NULL,
                    IsActive BIT NOT NULL DEFAULT 1,
                    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                    ModifiedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE()
                );
                INSERT INTO Applications (Name, Description, Notes, Url, IsActive)
                VALUES ('RubberJoins', 'Mobility tracking app - daily stretching and exercise routines with progress tracking', NULL, 'https://rubberjoins-app.azurewebsites.net', 1);
            END

            -- Deactivate the old RubberJoins app (replaced by RubberJointsAI)
            UPDATE Applications
            SET IsActive = 0,
                ModifiedDate = GETUTCDATE()
            WHERE Name = 'RubberJoins';

            -- Add RubberJointsAI if not exists
            IF NOT EXISTS (SELECT 1 FROM Applications WHERE Name = 'RubberJointsAI')
            BEGIN
                INSERT INTO Applications (Name, Description, Notes, Url, IsActive)
                VALUES ('RubberJointsAI', 'Because your joints shouldn''t sound like a bowl of Rice Krispies when you stand up. AI-powered mobility coaching that keeps you moving like you''re 25 — even if your knees disagree.', NULL, 'https://app.1000problems.com', 1);
            END
            ELSE
            BEGIN
                UPDATE Applications
                SET Description = 'Because your joints shouldn''t sound like a bowl of Rice Krispies when you stand up. AI-powered mobility coaching that keeps you moving like you''re 25 — even if your knees disagree.',
                    ImageUrl = '/images/rubberjoints-logo.svg',
                    IsActive = 1,
                    ModifiedDate = GETUTCDATE()
                WHERE Name = 'RubberJointsAI';

            -- Add B3tz if not exists
            IF NOT EXISTS (SELECT 1 FROM Applications WHERE Name = 'B3tz')
            BEGIN
                INSERT INTO Applications (Name, Description, Notes, Url, ImageUrl, IsActive)
                VALUES ('B3tz', 'Finally, a place to put your big mouth to work. Make bold predictions, challenge your friends, and find out who actually knows what they''re talking about -- spoiler: it''s probably not you.', NULL, 'https://b3tz.1000problems.com', '/images/b3tz-logo.svg', 1);
            END
            ELSE
            BEGIN
                UPDATE Applications
                SET Description = 'Finally, a place to put your big mouth to work. Make bold predictions, challenge your friends, and find out who actually knows what they''re talking about -- spoiler: it''s probably not you.',
                    ImageUrl = '/images/b3tz-logo.svg',
                    Url = 'https://b3tz.1000problems.com',
                    IsActive = 1,
                    ModifiedDate = GETUTCDATE()
                WHERE Name = 'B3tz';
            END
            END";

        using var command = new SqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }
}
