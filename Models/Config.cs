namespace PassPort.Models
{
    public class Config
    {
        public string? LogFile { get; set; }

        public Dictionary<string, Node> Graph { get; } = new();
    }
}
