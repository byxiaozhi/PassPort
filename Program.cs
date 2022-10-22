using PassPort.Interfaces;
using PassPort.Models;
using PassPort.Utilities;
using System.CommandLine;

namespace PassPort.Services
{
    public class Program
    {
        public static Config? Config { get; private set; }

        public static async Task Main(string[] args)
        {
            var rootCommand = new RootCommand(description: "PassPort is a fast traffic processing server.");
            var configOption = new Option<FileInfo?>(name: "--config");
            rootCommand.AddOption(configOption);
            rootCommand.SetHandler(Initialize, configOption);
            await rootCommand.InvokeAsync(args);
            await Task.Delay(int.MaxValue);
        }

        private static void Initialize(FileInfo? configFile)
        {
            Console.WriteLine("Loading config...");
            if (configFile == null)
                throw new ArgumentNullException(nameof(configFile));
            Config = ConfigLoader.LoadConfig(configFile.FullName);

            Console.WriteLine("Initialize module...");
            foreach (var node in Config.Graph.Values)
                node.Module.Initialize(node);
        }
    }
}
