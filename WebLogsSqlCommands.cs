using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;

namespace Fenton.WebLogImporter;

public class WebLogsSqlCommands
{
    private readonly FileHeader _header;
    private readonly OutFile _file;

    public WebLogsSqlCommands(FileHeader header, OutFile file)
    {
        _header = header;
        _file = file;
    }

    public SqlCommand GetBulkInsertCommand(SqlConnection connection)
    {
        string text = @"
BULK INSERT LogEntry FROM '" + _file.Value + "' " +
"WITH (FIRSTROW = 2, FIELDTERMINATOR = ' ', ROWTERMINATOR = '\n', MAXERRORS = 10)";
        return GetCommand(connection, text);
    }

    public SqlCommand GetDropObjectsCommand(SqlConnection connection)
    {
        string text = @"
IF OBJECT_ID('dbo.LogEntry', 'U') IS NOT NULL DROP TABLE dbo.LogEntry 
IF OBJECT_ID('dbo.RoundToMinutes', 'FN') IS NOT NULL DROP FUNCTION dbo.RoundToMinutes";
        return GetCommand(connection, text);
    }

    public SqlCommand GetCreateObjectsCommand(SqlConnection connection)
    {
        string text = @"
CREATE FUNCTION dbo.RoundToMinutes(@date DATETIME2, @time DATETIME2, @minutes INT) RETURNS SMALLDATETIME AS
BEGIN
	SELECT @date = CAST(@date AS DATETIME) + CAST(@time AS DATETIME)
	RETURN DATEADD(MINUTE, (DATEDIFF(MINUTE, 0, @date ) / @minutes) * @minutes, 0)
END";
        return GetCommand(connection, text);
    }

    [SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "<Pending>")]
    public SqlCommand GetTableCommand(SqlConnection connection)
    {
        string text = CreateTableQuery(_header.Value);
        return GetCommand(connection, text);
    }

    private static string CreateTableQuery(string header)
    {
        IList<string> fieldDefinitions = new List<string>();
        string[] fieldNames = header.Substring("#Fields: ".Length).Split(null);

        IDictionary<string, string> fieldTypes = new Dictionary<string, string>
        {
            { "date",            "[date]              DATETIME2       NULL" },
            { "time",            "[time]              DATETIME2       NULL" },
            { "s-ip",            "[s_ip]              [NVARCHAR](100) NULL" },
            { "cs-method",       "[cs_method]         [NVARCHAR](50)  NULL" },
            { "cs-uri-stem",     "[cs_uri_stem]       [NVARCHAR](MAX) NULL" },
            { "cs-uri-query",    "[cs_uri_query]      [NVARCHAR](MAX) NULL" },
            { "s-port",          "[s_port]            [INT]           NULL" },
            { "cs-username",     "[cs_username]       [NVARCHAR](100) NULL" },
            { "c-ip",            "[c_ip]              [NVARCHAR](100) NULL" },
            { "cs(User-Agent)",  "[cs_User_Agent]     [NVARCHAR](MAX) NULL" },
            { "cs(Referer)",     "[cs_Referer]        [NVARCHAR](MAX) NULL" },
            { "cs-host",         "[cs_host]           [NVARCHAR](MAX) NULL" },
            { "sc-bytes",        "[sc_bytes]          [INT]           NULL" },
            { "sc-status",       "[sc_status]         [INT]           NULL" },
            { "sc-substatus",    "[sc_substatus]      [INT]           NULL" },
            { "sc-win32-status", "[sc_win32_status]   [INT]           NULL" },
            { "time-taken",      "[time_taken]        [INT]           NULL" },
            { "X-Forwarded-For", "[X_Forwarded_For]   [NVARCHAR](100) NULL" },
        };

        int unknownIndex = 0;
        foreach (var field in fieldNames)
        {
            if (fieldTypes.ContainsKey(field))
            {
                fieldDefinitions.Add(fieldTypes[field]);
            }
            else
            {
                fieldDefinitions.Add($"[col{++unknownIndex}] [NVARCHAR](MAX) NULL");
            }
        }
        fieldDefinitions.Add("INDEX cci CLUSTERED COLUMNSTORE");

        string columns = string.Join($",{Environment.NewLine}", fieldDefinitions);
        string query = $"CREATE TABLE [dbo].[LogEntry]({Environment.NewLine}{columns}{Environment.NewLine}) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

        return query;
    }

    [SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "The _outFile item is not user defined.")]
    private SqlCommand GetCommand(SqlConnection connection, string commandText)
    {
        return new SqlCommand()
        {
            CommandText = commandText,
            CommandType = CommandType.Text,
            Connection = connection
        };
    }
}
