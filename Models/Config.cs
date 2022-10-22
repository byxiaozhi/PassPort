namespace PassPort.Models
{
    public class Config
    {
        public string? LogFile { get; }

        public IReadOnlyDictionary<string, Node> Chains { get; }

        public Config(string? logFile, IReadOnlyDictionary<string, Node> chains)
        {
            LogFile = logFile;
            Chains = chains;
        }
    }
}
