using System.Data.SqlClient;

namespace Fenton.WebLogImporter;

public interface IWebLogsDatabase
{
    void BulkLoadData();
    void Configure();
}

public class WebLogsDatabase : IWebLogsDatabase
{
    private readonly string _db = "Server=.;Database=WebLogs;Trusted_Connection=True;MultipleActiveResultSets=true;Pooling=true";
    private readonly WebLogsSqlCommands _commands;

    public WebLogsDatabase(FileHeader header, OutFile file)
    {
        _commands = new WebLogsSqlCommands(header, file);
    }

    public void Configure()
    {
        DropObjects();
        CreateLogEntryTable();
        CreateObjects();
    }

    public void BulkLoadData()
    {
        LoadData();
    }

    private void LoadData()
    {
        using var connection = new SqlConnection(_db);
        using SqlCommand command = _commands.GetBulkInsertCommand(connection);
        connection.Open();
        command.CommandTimeout = 3600; // 3600 seconds = 1 hour
        command.ExecuteNonQuery();
    }

    private void CreateObjects()
    {
        using var connection = new SqlConnection(_db);
        using SqlCommand command = _commands.GetCreateObjectsCommand(connection);
        connection.Open();
        command.ExecuteNonQuery();
    }

    private void CreateLogEntryTable()
    {
        using var connection = new SqlConnection(_db);
        using SqlCommand command = _commands.GetTableCommand(connection);
        connection.Open();
        command.ExecuteNonQuery();
    }

    private void DropObjects()
    {
        using var connection = new SqlConnection(_db);
        using SqlCommand command = _commands.GetDropObjectsCommand(connection);
        connection.Open();
        command.ExecuteNonQuery();
    }
}
