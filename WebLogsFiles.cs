using System;
using System.Collections.Generic;
using System.IO;

namespace Fenton.WebLogImporter;

public interface IWebLogsFiles
{
    OutFile Configure();
    FileHeader ProcessFiles();
}

public class WebLogsFiles : IWebLogsFiles
{
    private const string _in = "c:\\Temp\\Logs\\In";
    private const string _out = "c:\\Temp\\Logs\\Out";
    private const string _outFile = "c:\\Temp\\Logs\\Out\\out.log";
    private const string _comment = "#";
    private const string _header = "#Fields:";

    public OutFile Configure()
    {
        CreateDirectories();
        RemoveOldFiles();
        return new OutFile(_outFile);
    }

    public FileHeader ProcessFiles()
    {
        bool headerRowProcessed = false;
        string headerRow = string.Empty;

        using StreamWriter outfile = new StreamWriter(File.Create(_outFile));

        foreach (string file in Directory.GetFiles(_in))
        {
            Console.WriteLine(file);

            using FileStream fileStream = new FileStream(file, FileMode.Open);
            using StreamReader reader = new StreamReader(fileStream);

            string line = reader.ReadLine();

            while (line != null)
            {
                if (line.StartsWith(_comment))
                {
                    if (!headerRowProcessed && line.StartsWith(_header))
                    {
                        headerRowProcessed = true;
                        headerRow = line;
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

        return new FileHeader(headerRow);
    }

    private static void RemoveOldFiles()
    {
        if (File.Exists(_outFile))
        {
            File.Delete(_outFile);
        }
    }

    private static void CreateDirectories()
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
    }
}
