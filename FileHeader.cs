namespace Fenton.WebLogImporter
{
    public struct FileHeader
    {
        public FileHeader(string value)
        {
            Value = value;
        }

        public string Value { get; private set; }
    }
}
