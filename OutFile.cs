namespace Fenton.WebLogImporter;

public struct OutFile
{
    public OutFile(string value)
    {
        Value = value;
    }

    public string Value { get; private set; }
}
