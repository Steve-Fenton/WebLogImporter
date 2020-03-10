using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Fenton.WebLogImporter
{
    class Program
    {
        private static readonly string _in = "c:\\Temp\\Logs\\In";
        private static readonly string _out = "c:\\Temp\\Logs\\Out";
        private static readonly string _outFile = "c:\\Temp\\Logs\\Out\\out.log";
        private static readonly string _headerStart = "#";
        private static readonly string _fieldStart = "#Fields:";
        private static readonly string _db = "Server=.;Database=WebLogs;Trusted_Connection=True;MultipleActiveResultSets=true";

        static void Main(string[] args)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            
            ConfigureFileSystem();

            CombineAndCleanseFiles();

            EmptyTable();

            BulkLoadData();
            
            timer.Stop();

            Console.WriteLine("Elapsed time {0} ms", timer.ElapsedMilliseconds);
        }

        private static void CombineAndCleanseFiles()
        {
            bool headerRowProcessed = false;

            using (StreamWriter outfile = new StreamWriter(File.Create(_outFile)))
            {
                foreach (var file in Directory.GetFiles(_in))
                {
                    Console.WriteLine(file);

                    using FileStream fileStream = new FileStream(file, FileMode.Open);
                    using StreamReader reader = new StreamReader(fileStream);

                    string line = reader.ReadLine();

                    while (line != null)
                    {
                        if (line.StartsWith(_headerStart))
                        {
                            if (!headerRowProcessed && line.StartsWith(_fieldStart))
                            {
                                headerRowProcessed = true;
                                outfile.WriteLine(line);
                            }
                        }
                        else
                        {
                            outfile.WriteLine(line);
                        }

                        line = reader.ReadLine();
                    }
                }

                outfile.Close();
            }
        }

        [SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "The _outFile item is not user defined.")]
        private static void BulkLoadData()
        {
            using (var connection = new SqlConnection(_db))
            {
                using (var command = new SqlCommand()
                {
                    CommandText = "BULK INSERT LogEntry FROM '" + _outFile + "' WITH (FIRSTROW = 2, FIELDTERMINATOR = ' ', ROWTERMINATOR = '\n', MAXERRORS = 10)",
                    CommandType = CommandType.Text,
                    Connection = connection
                })
                {
                    connection.Open();

                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        private static void EmptyTable()
        {
            using (var connection = new SqlConnection(_db))
            {
                using (var command = new SqlCommand()
                {
                    CommandText = "TRUNCATE TABLE LogEntry",
                    CommandType = CommandType.Text,
                    Connection = connection
                })
                {
                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        private static void ConfigureFileSystem()
        {
            IList<string> directories = new List<string>
            {
                "c:\\Temp",
                "c:\\Temp\\Logs",
                _in,
                _out,
            };

            foreach (var directory in directories)
            {
                Directory.CreateDirectory(directory);
            }

            if (File.Exists(_outFile))
            {
                File.Delete(_outFile);
            }
        }
    }
}
