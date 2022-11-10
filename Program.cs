using System;
using System.Diagnostics;

namespace Fenton.WebLogImporter;

public static class Program
{
    public static void Main(string[] args)
    {
        TimedOperation(() =>
        {
            IWebLogsFiles files = new WebLogsFiles();
            OutFile outFile = files.Configure();
            FileHeader header = files.ProcessFiles();

            IWebLogsDatabase db = new WebLogsDatabase(header, outFile);
            db.Configure();
            db.BulkLoadData();
        });

#if debug
        Console.ReadKey();
#endif
    }

    private static void TimedOperation(Action action)
    {
        Stopwatch timer = new Stopwatch();
        timer.Start();

        action();

        timer.Stop();
        Console.WriteLine("Elapsed time {0} ms", string.Format("{0:N0}", timer.ElapsedMilliseconds));
    }
}
